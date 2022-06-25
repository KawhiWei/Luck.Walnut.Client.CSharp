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
        /// <param name="appId"></param>
        private void OnProjectConfigChanged(string appId)
        {
            var configs= LuckWalnutConfigCenterHelper.GetConfig();
            Data= LuckWalnutJsonConfigurationJsonParser.Parse(configs,Source.LuckWalnutConfig.AppId);
        }

        public override void Load()
        {
            var configs= LuckWalnutConfigCenterHelper.GetConfig();
            Data= LuckWalnutJsonConfigurationJsonParser.Parse(configs,Source.LuckWalnutConfig.AppId);
            base.Load();
        }

    }
}
