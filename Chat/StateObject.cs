using System.Net.Sockets;
using System.Text;
using System.Collections.Concurrent;

namespace API.Chat;

// [GNS301_Require] StateObject giữ buffer, socket và connection info giữa các callback async.
public sealed class StateObject
{
    public StateObject(TcpClient client, int bufferSize)
    {
        Id = Guid.NewGuid();
        Client = client;
        Stream = client.GetStream();
        Buffer = new byte[bufferSize];
    }

    public Guid Id { get; }
    public TcpClient Client { get; }
    public NetworkStream Stream { get; }
    public byte[] Buffer { get; }
    public StringBuilder PendingText { get; } = new();
    public ConcurrentQueue<byte[]> Outgoing { get; } = new();
    public int WriteInProgress;
    public string Username { get; set; } = string.Empty;
    public bool IsAuthenticated { get; set; }
}
