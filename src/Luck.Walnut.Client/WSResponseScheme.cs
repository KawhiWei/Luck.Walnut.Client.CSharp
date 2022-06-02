namespace Luck.Walnut.Client;

public class WSResponseScheme
{
    /// <summary>
    /// Response Id with request consistent 
    /// </summary>
    public string Id { get; set; } = default!;

    /// <summary>
    /// Response status.
    /// Success:0,Application Error:1,NotFoundTarget:2
    /// </summary>
    public int Status { get; set; } = default!;

    /// <summary>
    /// Response message
    /// </summary>
    public string Msg { get; set; } = default!;

    /// <summary>
    /// Request time tick
    /// </summary>
    public long RequestTime { get; set; } = default!;

    /// <summary>
    /// Handle complate time tick
    /// </summary>
    public long ComplateTime { get; set; } = default!;

    /// <summary>
    /// Response body
    /// </summary>
    public object Body { get; set; } = default!;
}