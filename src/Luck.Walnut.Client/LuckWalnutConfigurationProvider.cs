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
        }


        public override void Load()
        {
            var test= LuckWalnutConfigCenterHelper.GetConfig();
            Data= LuckWalnutJsonConfigurationJsonParser.Parse(test,Source.LuckWalnutConfig.AppId);
            base.Load();
        }

    }
}
