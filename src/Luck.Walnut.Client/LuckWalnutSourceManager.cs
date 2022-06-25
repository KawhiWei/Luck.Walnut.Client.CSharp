namespace Luck.Walnut.Client
{
    public class LuckWalnutSourceManager: ILuckWalnutSourceManager
    {
        private readonly LuckWalnutConfig _luckWalnutConfig;
        public LuckWalnutSourceManager(LuckWalnutConfig luckWalnutConfig)
        {
            _luckWalnutConfig = luckWalnutConfig;   
        }

        public Task<ProjectConfigAdapter> GetProjectConfigs()=> LuckWalnutSourceGrpcService.GetProjectConfigForResetFulApi(_luckWalnutConfig.ServerUri, _luckWalnutConfig.AppId, _luckWalnutConfig.Environment);
    }
}
