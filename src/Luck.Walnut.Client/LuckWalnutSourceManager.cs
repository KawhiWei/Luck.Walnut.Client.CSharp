using Grpc.Net.Client;
using Luck.Walnut.V1;

namespace Luck.Walnut.Client
{
    public class LuckWalnutSourceManager: ILuckWalnutSourceManager
    {
        private readonly string _appId;
        private readonly string _environment;

        public LuckWalnutSourceManager(string appId, string environment)
        {
            _appId = appId;
            _environment = environment;
        }

        public async Task<IEnumerable<LuckWalnutConfigAdapter>> GetProjectConfigs()
        {
            using var channel = GrpcChannel.ForAddress("http://localhost:5000");
            var client = new GetConfig.GetConfigClient(channel);
            var request = new ApplicationConfigRequest() { AppId = _appId, EnvironmentName = _environment };
            var results = await client.GetAppliactionConfigAsync(request);

            return results.Result.Select(config => new LuckWalnutConfigAdapter
            {

                Key = config.Key,
                Value = config.Value,
                Type = config.Type,
            });
            //var dic = LuckWalnutJsonConfigurationFileParser.Parse(results.Result.First().Value);
        }
    }
}
