namespace Luck.Walnut.Client
{
    public interface ILuckWalnutSourceManager
    {
        Task<ProjectConfigAdapter> GetProjectConfigs();
    }
}
