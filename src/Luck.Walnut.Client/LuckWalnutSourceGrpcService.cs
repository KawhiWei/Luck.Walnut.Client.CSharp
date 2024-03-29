﻿using System.Net;
using System.Text.Json;
using Grpc.Net.Client;
using Luck.Walnut.V1;

namespace Luck.Walnut.Client
{


    internal  class LuckWalnutSourceGrpcService
    {

        public static async Task<IEnumerable<LuckWalnutConfigAdapter>> GetProjectConfigs(string serverUri,string appId, string environment)
        {
            AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
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

        public static async Task<ProjectConfigAdapter?> GetProjectConfigForResetFulApi(string serverUri,string appId, string environment)
        {
            
            using (var client=new HttpClient())
            {
               var response= await client.GetAsync($"{serverUri}/walnut/api/environment/{appId}/{environment}/config");
                if(response.StatusCode!=HttpStatusCode.OK)
                {
                    throw new Exception($"{response.StatusCode}");
                }
                var content = await response.Content.ReadAsStringAsync();
               var result= content.Deserialize<Response<ProjectConfigAdapter>>(new JsonSerializerOptions()
               {
                   PropertyNameCaseInsensitive=true,
               });
               return result?.Result ?? new ProjectConfigAdapter();

            }
        }
        
    }
    public  class  Response<T>
    {
        public T Result { get; set; } = default!;

        public bool Success { get; set; } = default!;
        
        public  string ErrorMessage { get; set; } = default!;
        
        public  string ErrorCode { get; set; } = default!;
    }
}
