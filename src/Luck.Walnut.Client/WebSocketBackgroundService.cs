using Luck.Walnut.Client.WebSocketClients;
using Microsoft.Extensions.Hosting;

namespace Luck.Walnut.Client
{
    public class WebSocketBackgroundService : BackgroundService
    {

        
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {

            
            return Task.CompletedTask;
        }

        private void WatchingLuckWalnutConfig()
        {


            //Task.Factory.StartNew();
            //do
            //{
                



            //} while (true);



        }

    }
}
