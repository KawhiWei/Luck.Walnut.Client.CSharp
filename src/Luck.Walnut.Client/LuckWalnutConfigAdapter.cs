namespace Luck.Walnut.Client
{
    public class LuckWalnutConfigAdapter
    {
        public string Key { get; set; } = default!;

        public string Value { get; set; } = default!;

        public string Type { get; set; } = default!;
    }

    public class ProjectConfigAdapter
    {
        /// <summary>
        /// 应用标识
        /// </summary>
        public string AppId { get; set; } = default!;

        /// <summary>
        /// 版本号
        /// </summary>
        public string Version { get; set; } = default!;

        /// <summary>
        /// 环境标识
        /// </summary>
        public string EnvironmentName { get; set; } = default!;
        
        /// <summary>
        /// 配置列表
        /// </summary>
        public  List<LuckWalnutConfigAdapter> Configs { get; set; } = default!;
    }
}
