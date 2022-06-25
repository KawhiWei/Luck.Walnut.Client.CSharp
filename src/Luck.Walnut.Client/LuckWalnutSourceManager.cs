using System.Runtime.CompilerServices;
using System.Text;
using Luck.Walnut.Client.WebSocketClients;

namespace Luck.Walnut.Client
{
    public class LuckWalnutSourceManager : ILuckWalnutSourceManager
    {
        private readonly LuckWalnutConfig _luckWalnutConfig;

        /// <summary>
        /// WebSocket客户端
        /// </summary>
        private WebSocketClient _client;

        /// <summary>
        /// 应用统一配置源发生变更事件，给ConfigCenterHelper注册
        /// </summary>
        public event Func<Task>? ProjectConfigSourceChanged;


        public LuckWalnutSourceManager(LuckWalnutConfig luckWalnutConfig)
        {
            _luckWalnutConfig = luckWalnutConfig;
        }

        public async Task<ProjectConfigAdapter?> GetProjectConfigs()
        {
            
            var result= await LuckWalnutSourceGrpcService.GetProjectConfigForResetFulApi(_luckWalnutConfig.ServerUri,
                _luckWalnutConfig.AppId, _luckWalnutConfig.Environment);
            return result;
        }


        public void Watching()
        {
            Task.Run(() => Connecting());
        }


        private async Task Connecting()
        {
            try
            {
                if (_client is null)
                {
                    for (int i = 0; i < int.MaxValue; i++)
                    {
                        _client = new WebSocketClient().OnConnected(OnConnected);
                        if (await _client.ConnectAsync(new Uri(_luckWalnutConfig.WebSocketUri), true))
                        {
                            break;
                        }

                        _client.Dispose();
                        await Task.Delay(i * 100);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// 链接成功回调
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private Task OnConnected(WebSocketClient client)
        {
            if (_client is not null)
            {
                _client.OnReceiveMessage(OnReceiveMessage)
                    .OnReceiveData(OnReceiveData)
                    .OnDisconnected(OnDisconnected);
                client.StartListen(); //此时的_client跟client不一样么?
                return SendWatchProjectAsync();
            }

            throw new NullReferenceException("$client is null");
        }

        /// <summary>
        /// 接受服务端发送数据处理
        /// </summary>
        /// <param name="client"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        private Task OnReceiveData(WebSocketClient client, Stream stream)
        {
            using (var sr = new StreamReader(stream, Encoding.UTF8))
            {
                return OnReceiveMessage(client, sr.ReadToEnd());
            }
        }

        /// <summary>
        /// 接受服务端发送消息处理
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        private async Task OnReceiveMessage(WebSocketClient client, string message)
        {
            try
            {
                var cmdResult = message.Deserialize<WSResponseScheme>();
                if (cmdResult?.Body.ToString() == "ok")
                {
                    return;
                }

                if (cmdResult?.Body.ToString() == "reload")
                {
                    await OnProjectConfigChanged();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                // _logger.LogError($"处理websocket返回消息失败！", exception);
            }
        }

        private async Task SendWatchProjectAsync()
        {
            await Task.CompletedTask;
            var appId = $"{_luckWalnutConfig.AppId}"; //{Guid.NewGuid().ToString().Replace("-", "")}
            var requestCmd = new Commad
            {
                TargetAction = "im.clientlogin", Body = new
                {
                    appId = appId
                }
            };
            if (_client is null)
                throw new NullReferenceException("$client is null");
            var json = requestCmd.Serialize();
            await _client.SendMessageAsync(json); //发送注册请求
        }

        private Task OnDisconnected(WebSocketClient client)
        {
            return Connecting(); //websocket连接中断后尝试重连？
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task OnProjectConfigChanged()
        {
            if (ProjectConfigSourceChanged is not null)
            {
                try
                {
                    await ProjectConfigSourceChanged();
                }
                catch (Exception ex)
                {
                    // Console.WriteLine(ex);
                }
            }
        }
    }
}