using Microsoft.Extensions.Configuration;

namespace Luck.Walnut.Client
{
    public class LuckWalnutConfigurationProvider : ConfigurationProvider
    {
        /// <summary>
        /// 
        /// </summary>
        public LuckWalnutConfigurationSource Source { get; } = default!;

        /// <summary>
        /// 
        /// </summary>
        public LuckWalnutConfigCenterHelper LuckWalnutConfigCenterHelper { get; }
        public LuckWalnutConfigurationProvider(LuckWalnutConfigurationSource source)
        {
            Source = source;
            LuckWalnutConfigCenterHelper = new LuckWalnutConfigCenterHelper(Source.LuckWalnutConfig);
            if (Source.ReloadOnChange)
            {
                LuckWalnutConfigCenterHelper.ProjectConfigChanged += OnProjectConfigChanged;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnProjectConfigChanged()
        {
            var configs= LuckWalnutConfigCenterHelper.GetNewProjectConfigs();
            Data= LuckWalnutJsonConfigurationJsonParser.Parse(configs,Source.LuckWalnutConfig.AppId);
            Console.WriteLine($"接收到消息刷新完成{DateTime.Now}");
            OnReload();
        }

        public override void Load()
        {
            var configs= LuckWalnutConfigCenterHelper.GetConfig();
            Data= LuckWalnutJsonConfigurationJsonParser.Parse(configs,Source.LuckWalnutConfig.AppId);
            base.Load();
        }

    }
}
