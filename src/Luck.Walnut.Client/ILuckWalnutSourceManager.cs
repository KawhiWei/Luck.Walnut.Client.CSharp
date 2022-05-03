namespace Luck.Walnut.Client
{
    public interface ILuckWalnutSourceManager
    {
        Task<IEnumerable<LuckWalnutConfigAdapter>> GetProjectConfigs();
    }
}
