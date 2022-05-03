using Luck.Walnut.Client;

namespace Microsoft.Extensions.Configuration
{
    public static class LuckWalnutConfigurationExtension
    {
        public static IConfigurationBuilder AddLuckWalnutConfig(this IConfigurationBuilder builder, IConfiguration configuration)
        {
            return AddLuckWalnutConfig(builder, configuration, null);
        }

        public static IConfigurationBuilder AddLuckWalnutConfig(this IConfigurationBuilder builder, IConfiguration configuration, Action<LuckWalnutConfigurationSource>? configureSource = null)
        {
            Action<LuckWalnutConfigurationSource> createSource = source =>
            {
                source.AppId = /*configuration["APP_UK"]*/"walnut";
                source.Environment = /*configuration["ENVIRONMENT"]*/"test";
                if (configureSource is not null)
                {
                    configureSource(source);
                }
            };
            return builder.Add(createSource);
        }
    }
}
