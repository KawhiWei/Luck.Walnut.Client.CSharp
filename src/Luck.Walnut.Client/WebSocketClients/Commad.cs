namespace Luck.Walnut.Client.WebSocketClients;

/// <summary>
/// ws请求指令
/// </summary>
public class Commad
{
    /// <summary>
    /// 在多路复用中需要保证Id的唯一性
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Request target
    /// </summary>
    public string TargetAction { get; set; } = default!;

    /// <summary>
    /// Request context
    /// </summary>
    public object Body { get; set; }= default!;
}