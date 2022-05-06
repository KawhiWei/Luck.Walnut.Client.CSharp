using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace Luck.Walnut.Client.WebSocketClients
{
    public class WebSocketClient : IDisposable
    {
        private readonly ILogger _logger;
        private const int ReceiveChunkSize = 1024;
        private ClientWebSocket _ws;
        private Uri _uri;
        private bool disposedValue = false; // 要检测冗余调用
        private Func<WebSocketClient, Task> _onConnected;
        private Func<WebSocketClient, Task> _onDisconnected;
        private Func<WebSocketClient, string, Task> _onReceiveMessage;
        private Func<WebSocketClient, Stream, Task> _onReceiveData;
        /// <summary>
        /// 连接超时时间（默认3秒）
        /// </summary>
        public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(3);

        /// <summary>
        /// 发送超时时间（默认3秒）
        /// </summary>
        public TimeSpan SendTimeout { get; set; } = TimeSpan.FromSeconds(3);

        /// <summary>
        /// 创建 WebSocketClient 类的实例。
        /// </summary>
        public WebSocketClient(IServiceProvider serviceProvider)
        {
            _ws = new ClientWebSocket();
            _ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(5);
            _logger = serviceProvider.GetService<ILogger>();
        }

        /// <summary>
        /// 连接到WebSocket服务器以作为异步操作。
        /// </summary>
        /// <param name="uri">要连接到的 WebSocket 服务器的 URI。必须以'ws://'或'wss://'开头</param>
        /// <param name="ignoreScheme">是否忽略Scheme，以ws协议连接</param>
        /// <returns>返回 System.Threading.Tasks.Task。表示异步操作的任务对象。</returns>
        public async Task<bool> ConnectAsync(Uri uri, bool ignoreScheme = false)
        {
            _uri = uri;
            if (ignoreScheme)
            {
                var uriBuilder = new UriBuilder(uri);
                uriBuilder.Scheme = UriScheme.Ws;
                _uri = uriBuilder.Uri;
            }
            try
            {
                var cts = new CancellationTokenSource(ConnectTimeout);
                await _ws.ConnectAsync(_uri, cts.Token);
                await CallOnConnected();
                return true;
            }
            catch (ArgumentException)
            {
                var errorMessage = "必须以'ws://'或'wss://'开头，也许你应该设置ignoreScheme参数为ture";
                _logger.LogError($"uri{errorMessage}");
                throw new ArgumentException(errorMessage, nameof(uri));
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning($"连接到{uri}超时");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ConnectAsync");
                return false;
            }
        }

        /// <summary>
        /// 设置在连接建立后调用的函数。
        /// </summary>
        /// <param name="onConnected">被调用的函数</param>
        /// <returns>WebSocketClient类实例</returns>
        public WebSocketClient OnConnected(Func<WebSocketClient, Task> onConnected)
        {
            _onConnected = onConnected;
            return this;
        }

        /// <summary>
        /// 设置在连接断开后调用的函数。
        /// </summary>
        /// <param name="onDisconnected">被调用的函数</param>
        /// <returns>WebSocketClient类实例</returns>
        public WebSocketClient OnDisconnected(Func<WebSocketClient, Task> onDisconnected)
        {
            _onDisconnected = onDisconnected;
            return this;
        }

        /// <summary>
        /// 设置在接收到消息后调用的函数。
        /// </summary>
        /// <param name="onReceiveMessage">被调用的函数</param>
        /// <returns>WebSocketClient类实例</returns>
        public WebSocketClient OnReceiveMessage(Func<WebSocketClient, string, Task> onReceiveMessage)
        {
            _onReceiveMessage = onReceiveMessage;
            return this;
        }

        /// <summary>
        /// 设置在接收到数据后调用的函数。
        /// </summary>
        /// <param name="onReceiveData">被调用的函数</param>
        /// <returns>WebSocketClient类实例</returns>
        public WebSocketClient OnReceiveData(Func<WebSocketClient, Stream, Task> onReceiveData)
        {
            _onReceiveData = onReceiveData;
            return this;
        }

        /// <summary>
        /// 异步发送消息到WebSocket服务器.
        /// </summary>
        /// <param name="message">发送的消息</param>
        public async Task<bool> SendMessageAsync(string message)
        {
            if (_ws.State != WebSocketState.Open)
                return false;

            var messageBuffer = Encoding.UTF8.GetBytes(message); // TODO reuse buffers

            try
            {
                var cts = new CancellationTokenSource(SendTimeout);
                await _ws.SendAsync(new ArraySegment<byte>(messageBuffer), WebSocketMessageType.Text, true, cts.Token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendMessageAsync");
                return false;
            }
            return true;
        }
        /// <summary>
        /// 异步发送数据到WebSocket服务器.
        /// </summary>
        /// <param name="fillToSend"></param>
        /// <returns></returns>
        public async Task<bool> SendDataAsync(Action<Stream> fillToSend)
        {
            if (_ws.State != WebSocketState.Open)
                return false;

            using (var ms = new MemoryStream())
            {
                try
                {
                    fillToSend(ms);// TODO reuse buffers
                    var messageBuffer = ms.ToArray();
                    var cts = new CancellationTokenSource(SendTimeout);
                    await _ws.SendAsync(new ArraySegment<byte>(messageBuffer), WebSocketMessageType.Binary, true, cts.Token);

                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "SendDataAsync");
                    return false;
                }
            }

        }

        /// <summary>
        /// 开始接受数据
        /// </summary>
        public async void StartListen()
        {
            var stream = new MemoryStream();
            var buffer = new ArraySegment<byte>(new byte[ReceiveChunkSize]);    // TODO reuse buffers

            while (_ws.State == WebSocketState.Open)
            {
                var result = await DoReceiveAsync(buffer);
                if (result == null)
                    break;

                stream.Write(buffer.Array, 0, result.Count);
                if (!result.EndOfMessage)
                    continue;

                stream.Position = 0;
                if (result.MessageType == WebSocketMessageType.Binary)
                {
                    await CallOnData(stream);
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    using (var sr = new StreamReader(stream, Encoding.UTF8))
                    {
                        await CallOnMessage(sr.ReadToEnd());
                    }
                }
                stream = new MemoryStream();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<WebSocketReceiveResult> DoReceiveAsync(ArraySegment<byte> buffer)
        {
            try
            {
                var result = await _ws.ReceiveAsync(buffer, CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    CallOnDisconnected();
                    return null;
                }
                return result;
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "DoReceiveAsync");
                CallOnDisconnected();
                //_ws.Dispose();
                return null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task CallOnMessage(string message)
        {
            if (_onReceiveMessage != null)
            {
                try
                {
                    await _onReceiveMessage(this, message);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "OnReceiveMessage");
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task CallOnData(Stream data)
        {
            if (_onReceiveData != null)
            {
                try
                {
                    await _onReceiveData(this, data);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "OnReceiveData");
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async void CallOnDisconnected()
        {
            try
            {
                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            }
            catch (Exception ex)
            {
                //_logger.LogWarning(ex, "CloseAsync");
            }

            if (_onDisconnected != null)
            {
                try
                {
                    await _onDisconnected(this);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "OnDisconnected");
                }
            }
            try
            {
                _ws.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Dispose");
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task CallOnConnected()
        {
            if (_onConnected != null)
            {
                try
                {
                    await _onConnected(this);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "OnConnected");
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _onConnected = null;
                    _onDisconnected = null;
                    _onReceiveMessage = null;
                    _onReceiveData = null;
                }

                if (_ws != null)
                {
                    try
                    {
                        _ws.Dispose();
                    }
                    catch (Exception ex)
                    {
                    }
                    _ws = null;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }


    internal class UriScheme
    {
        public const string Ws = "ws";
        public const string Wss = "wss";

    }

}
