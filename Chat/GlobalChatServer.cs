using API.Config;
using API.Services;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace API.Chat;

public sealed class GlobalChatServer : IHostedService, IDisposable
{
    private readonly ConcurrentDictionary<Guid, StateObject> _clients = new();
    private readonly GlobalChatSettings _settings;
    private readonly IJwtTokenService _tokens;
    private readonly ILogger<GlobalChatServer> _logger;
    private readonly GlobalChatProtocol _protocol;
    private TcpListener? _listener;
    private bool _stopping;

    public GlobalChatServer(
        IOptions<GlobalChatSettings> settings,
        IJwtTokenService tokens,
        ILogger<GlobalChatServer> logger)
    {
        _settings = settings.Value;
        _tokens = tokens;
        _logger = logger;
        _protocol = new GlobalChatProtocol(_settings, _tokens, Send, Broadcast);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        IPAddress address = IPAddress.Parse(_settings.Address);
        _listener = new TcpListener(address, _settings.Port);
        _listener.Start();

        // [GNS301_Require] BeginAcceptTcpClient không chặn request thread của ASP.NET.
        _listener.BeginAcceptTcpClient(AcceptCallback, null);
        _logger.LogInformation("Global chat listening on {Address}:{Port}.",
            _settings.Address, _settings.Port);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _stopping = true;
        _listener?.Stop();
        foreach (StateObject state in _clients.Values)
        {
            Disconnect(state);
        }
        return Task.CompletedTask;
    }

    private void AcceptCallback(IAsyncResult result)
    {
        if (_stopping || _listener is null)
        {
            return;
        }

        try
        {
            // [GNS301_Require] EndAcceptTcpClient hoàn tất accept callback và re-arm listener.
            TcpClient client = _listener.EndAcceptTcpClient(result);
            _listener.BeginAcceptTcpClient(AcceptCallback, null);

            var state = new StateObject(client, _settings.BufferSize);
            _clients[state.Id] = state;
            BeginRead(state);
        }
        catch (ObjectDisposedException) when (_stopping)
        {
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Global chat accept failed.");
            if (!_stopping)
            {
                _listener.BeginAcceptTcpClient(AcceptCallback, null);
            }
        }
    }

    private void BeginRead(StateObject state)
    {
        try
        {
            // [GNS301_Require] BeginRead xử lý từng client bất đồng bộ, không dùng vòng lặp blocking.
            state.Stream.BeginRead(
                state.Buffer,
                0,
                state.Buffer.Length,
                ReadCallback,
                state);
        }
        catch (Exception)
        {
            Disconnect(state);
        }
    }

    private void ReadCallback(IAsyncResult result)
    {
        var state = (StateObject)result.AsyncState!;
        try
        {
            // [GNS301_Require] EndRead phát hiện disconnect đột ngột bằng count=0/exception.
            int count = state.Stream.EndRead(result);
            if (count == 0)
            {
                Disconnect(state);
                return;
            }

            state.PendingText.Append(Encoding.UTF8.GetString(state.Buffer, 0, count));
            _protocol.ProcessPending(state);
            BeginRead(state);
        }
        catch (Exception)
        {
            Disconnect(state);
        }
    }

    private void Broadcast(string message)
    {
        foreach (StateObject state in _clients.Values)
        {
            if (state.IsAuthenticated)
            {
                Send(state, message);
            }
        }
    }

    private void Send(StateObject state, string message)
    {
        state.Outgoing.Enqueue(Encoding.UTF8.GetBytes(message));
        if (Interlocked.CompareExchange(ref state.WriteInProgress, 1, 0) == 0)
        {
            BeginNextWrite(state);
        }
    }

    private static void BeginNextWrite(StateObject state)
    {
        if (state.Outgoing.TryDequeue(out byte[]? data))
        {
            state.Stream.BeginWrite(data, 0, data.Length, EndWrite, state);
            return;
        }

        Interlocked.Exchange(ref state.WriteInProgress, 0);
        if (!state.Outgoing.IsEmpty &&
            Interlocked.CompareExchange(ref state.WriteInProgress, 1, 0) == 0)
        {
            BeginNextWrite(state);
        }
    }

    private static void EndWrite(IAsyncResult result)
    {
        var state = (StateObject)result.AsyncState!;
        try
        {
            state.Stream.EndWrite(result);
            BeginNextWrite(state);
        }
        catch
        {
            Interlocked.Exchange(ref state.WriteInProgress, 0);
        }
    }

    private void Disconnect(StateObject state)
    {
        if (!_clients.TryRemove(state.Id, out _))
        {
            return;
        }
        try
        {
            state.Stream.Close();
            state.Client.Close();
        }
        catch
        {
        }
    }

    public void Dispose() => _listener?.Stop();
}
