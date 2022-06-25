using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Luck.Walnut.Client
{
    public class LuckWalnutConfigurationSource : IConfigurationSource
    {
        /// <summary>
        /// 项目配置
        /// </summary>
        public LuckWalnutConfig LuckWalnutConfig { get; set; } = default!;
        /// <summary>
        /// 
        /// </summary>
        public bool ReloadOnChange { get; internal set; } = true;

        
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new LuckWalnutConfigurationProvider(this);
        }
    }
}
