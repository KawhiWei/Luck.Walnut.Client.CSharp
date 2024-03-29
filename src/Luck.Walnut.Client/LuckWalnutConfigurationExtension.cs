﻿using Google.Protobuf.WellKnownTypes;
using Luck.Walnut.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Configuration
{
    public static class LuckWalnutConfigurationExtension
    {
        public static IServiceCollection AddLuckWalnutConfig(this IServiceCollection services, IConfigurationBuilder configurationbuilder, IConfiguration configuration, Action<LuckWalnutConfig> action, Action<LuckWalnutConfigurationSource>? configureSource = null)
        {
            var luckWalnutConfig = new LuckWalnutConfig();

            action.Invoke(luckWalnutConfig);

            var appId = Environment.GetEnvironmentVariable("AppId");
            if (string.IsNullOrEmpty(appId))
            {
                throw new ArgumentNullException($"AppId未设置");
            }
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (string.IsNullOrEmpty(environment))
            {
                throw new ArgumentNullException($"[ASPNETCORE_ENVIRONMENT]未设置");
            }

            luckWalnutConfig.Environment = environment;
            luckWalnutConfig.AppId = appId;
            
            services.Configure<LuckWalnutConfig>(x =>
            {
                x.Environment = luckWalnutConfig.Environment;
                x.AppId = luckWalnutConfig.AppId;
                x.ServerUri = luckWalnutConfig.ServerUri;
                x.WebSocketUri = luckWalnutConfig.WebSocketUri;
            });
            configurationbuilder.AddLuckWalnutConfig(configuration, luckWalnutConfig, configureSource);
            // services.AddHostedService<WebSocketBackgroundService>();
            return services;

        }

        private static IConfigurationBuilder AddLuckWalnutConfig(this IConfigurationBuilder builder, IConfiguration configuration, LuckWalnutConfig luckWalnutConfig, Action<LuckWalnutConfigurationSource>? configureSource = null)
        {
            Action<LuckWalnutConfigurationSource> createSource = source =>
            {
                source.LuckWalnutConfig = luckWalnutConfig;
                if (configureSource is not null)
                {
                    configureSource(source);
                }
            };
            return builder.Add(createSource);
        }
    }
}
