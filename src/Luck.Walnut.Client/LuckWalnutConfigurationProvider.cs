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
            LuckWalnutConfigCenterHelper = new LuckWalnutConfigCenterHelper(source.AppId, source.Environment);
        }


        public override void Load()
        {

            var test= LuckWalnutConfigCenterHelper.GetConfig();
            Data= LuckWalnutJsonConfigurationJsonParser.Parse(test,Source.AppId);
            base.Load();
        }

    }
}
