using Microsoft.Extensions.Configuration;

namespace Luck.Walnut.Client
{
    public class LuckWalnutConfigurationSource : IConfigurationSource
    {
        /// <summary>
        /// 应用标识
        /// </summary>
        public string AppId { get; set; } = default!;

        /// <summary>
        /// 项目环境
        /// </summary>
        public string Environment { get; set; } = default!;
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
