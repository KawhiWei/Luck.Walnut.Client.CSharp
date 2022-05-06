using Grpc.Net.Client;
using Luck.Walnut.Client.WebSocketClients;
using Luck.Walnut.V1;

namespace Luck.Walnut.Client
{
    public class LuckWalnutSourceManager: ILuckWalnutSourceManager
    {
        private readonly LuckWalnutConfig _luckWalnutConfig;
        private WebSocketClient? _client=null;
        public LuckWalnutSourceManager(LuckWalnutConfig luckWalnutConfig)
        {
            _luckWalnutConfig = luckWalnutConfig;   
        }

        public Task<IEnumerable<LuckWalnutConfigAdapter>> GetProjectConfigs()=> LuckWalnutSourceGrpcService.GetProjectConfigs(_luckWalnutConfig.ServerUri, _luckWalnutConfig.AppId, _luckWalnutConfig.Environment);


    }
}
