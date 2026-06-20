namespace API.Config;

public sealed class GlobalChatSettings
{
    public string Address { get; set; } = "0.0.0.0";
    public int Port { get; set; } = 5001;
    public int BufferSize { get; set; } = 4096;
    public int MaxMessageLength { get; set; } = 512;
}
