using System.Text;
using Luck.Walnut.Client.WebSocketClients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Luck.Walnut.Client
{
    public class WebSocketBackgroundService : BackgroundService
    {
        private readonly LuckWalnutConfigCenterHelper _luckWalnutConfigCenterHelper;
        private WebSocketClient? _client = null;
        private readonly IServiceProvider _serviceProvider;
        private readonly LuckWalnutConfig _luckWalnutConfig;
        private  readonly  ILogger<WebSocketBackgroundService> _logger;
        public WebSocketBackgroundService(IServiceProvider serviceProvider, IOptions<LuckWalnutConfig> options,ILogger<WebSocketBackgroundService> logger)
        {
            _luckWalnutConfig = options.Value;
            _luckWalnutConfigCenterHelper = new LuckWalnutConfigCenterHelper(_luckWalnutConfig);
            _serviceProvider = serviceProvider;
            _logger = logger;

        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Watching();
            return Task.CompletedTask;
        }

        public void Watching()
        {
            Task.Factory.StartNew(() => Connecting());
            // Task.Factory.StartNew(() => Connecting());
        }

        private async Task Connecting()
        {
            for (int i = 0; i < int.MaxValue; i++)
            {
                _client = new WebSocketClient(_serviceProvider).OnConnected(OnConnected);
                if (await _client.ConnectAsync(new Uri(_luckWalnutConfig.WebSocketUri), true))
                {
                    break;
                }
                _client.Dispose();
                await Task.Delay(i * 100);
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
            throw  new NullReferenceException("$client is null");
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
            await Task.CompletedTask;
            try
            {
                var cmdResult = message.Deserialize<WSResponseScheme>();
                if (cmdResult?.Body.ToString() == "ok")
                {
                    return;
                }
                if(cmdResult?.Body.ToString() == "reload")
                {
                    
                    var test = _serviceProvider.CreateScope().ServiceProvider
                        .GetRequiredService<IConfigurationProvider>();
                    _luckWalnutConfigCenterHelper.OnProjectConfigSourceChanged();
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"处理websocket返回消息失败！", exception);
            }
            
        }

        private async Task SendWatchProjectAsync()
        {
            await Task.CompletedTask;
            var appId = $"{_luckWalnutConfig.AppId}";//{Guid.NewGuid().ToString().Replace("-", "")}
            var requestCmd = new Commad {  TargetAction = "im.clientlogin", Body = new 
            {
                appId=appId
            }};
            if(_client is null)
                throw  new NullReferenceException("$client is null");
            var json= requestCmd.Serialize();
            await _client.SendMessageAsync(json);  //发送注册请求
        }
        private Task OnDisconnected(WebSocketClient client)
        {
            return Connecting(); //websocket连接中断后尝试重连？
        }
    }
}