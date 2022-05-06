using Grpc.Net.Client;
using Luck.Walnut.V1;

namespace Luck.Walnut.Client
{


    internal  class LuckWalnutSourceGrpcService
    {

        public static async Task<IEnumerable<LuckWalnutConfigAdapter>> GetProjectConfigs(string serverUri,string appId, string environment)
        {
            using var channel = GrpcChannel.ForAddress(serverUri);//"http://localhost:5000"
            var client = new GetConfig.GetConfigClient(channel);
            var request = new ApplicationConfigRequest() { AppId = appId, EnvironmentName = environment };
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
