using API.Config;
using API.Services;
using System.Security.Claims;

namespace API.Chat;

public sealed class GlobalChatProtocol
{
    private readonly GlobalChatSettings _settings;
    private readonly IJwtTokenService _tokens;
    private readonly Action<StateObject, string> _send;
    private readonly Action<string> _broadcast;

    public GlobalChatProtocol(
        GlobalChatSettings settings,
        IJwtTokenService tokens,
        Action<StateObject, string> send,
        Action<string> broadcast)
    {
        _settings = settings;
        _tokens = tokens;
        _send = send;
        _broadcast = broadcast;
    }

    public void ProcessPending(StateObject state)
    {
        string pending = state.PendingText.ToString();
        int lineEnd;
        while ((lineEnd = pending.IndexOf('\n')) >= 0)
        {
            ProcessLine(state, pending[..lineEnd].TrimEnd('\r'));
            pending = pending[(lineEnd + 1)..];
        }
        state.PendingText.Clear();
        state.PendingText.Append(pending);
    }

    private void ProcessLine(StateObject state, string line)
    {
        if (!state.IsAuthenticated)
        {
            Authenticate(state, line);
            return;
        }

        string message = line.Trim();
        if (message.Length == 0)
        {
            return;
        }
        if (message.Length > _settings.MaxMessageLength)
        {
            message = message[.._settings.MaxMessageLength];
        }
        _broadcast($"CHAT|{state.Username}|{message}\n");
    }

    private void Authenticate(StateObject state, string line)
    {
        if (!line.StartsWith("AUTH|", StringComparison.Ordinal))
        {
            _send(state, "ERROR|AUTH_REQUIRED\n");
            return;
        }

        ClaimsPrincipal? principal = _tokens.ValidateToken(line[5..]);
        string? username = principal?.FindFirstValue(ClaimTypes.Name);
        if (string.IsNullOrWhiteSpace(username))
        {
            _send(state, "ERROR|INVALID_TOKEN\n");
            return;
        }

        state.Username = username;
        state.IsAuthenticated = true;
        _send(state, $"SYSTEM|CONNECTED|{username}\n");
        _broadcast($"SYSTEM|JOINED|{username}\n");
    }
}
