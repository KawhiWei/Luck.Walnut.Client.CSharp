using Grpc.Net.Client;
using Luck.Walnut.V1;

namespace Luck.Walnut.Client
{
    public class LuckWalnutConfigCenterHelper
    {
        private ManualResetEventSlim _manualResetEventSlim;
        private readonly LuckWalnutConfig _luckWalnutConfig;
        private string _serverUri;
        private IDictionary<string, IDictionary<string, string>> _projectsConfigs;
        private IDictionary<string, IDictionary<string, string>> _projectsVersions;
        private readonly ILuckWalnutSourceManager _luckWalnutSourceManager;
        public event Action? ProjectConfigChanged;

        public LuckWalnutConfigCenterHelper(LuckWalnutConfig luckWalnutConfig)
        {
            _manualResetEventSlim = new ManualResetEventSlim(false);
            _luckWalnutConfig = luckWalnutConfig;
            _luckWalnutSourceManager = new LuckWalnutSourceManager(_luckWalnutConfig);
            _projectsConfigs = new BlockingDictionary<string, IDictionary<string, string>>();
            _projectsVersions = new BlockingDictionary<string, IDictionary<string, string>>();
        }

        private void SetProjectsConfigs(ProjectConfigAdapter luckWalnutConfigs)
        {
            try
            {
                
                var configDic = luckWalnutConfigs.Configs?.ToDictionary(config => config.Key, config => config.Value);
                if(!string.IsNullOrEmpty(_luckWalnutConfig.AppId))
                {
                    _projectsConfigs[_luckWalnutConfig.AppId] = configDic??new Dictionary<string, string>();    
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void GetProjectConfigs()
        {
            var task = Task.Factory.StartNew(async () =>
            {
                try
                {
                    Exception? exception = null;
                    ProjectConfigAdapter? projectsConfigs = null;

                    for (int i = 0; i < 5; i++) //尝试多次，防止单次运行出错
                    {
                        try
                        {
                            projectsConfigs = await _luckWalnutSourceManager.GetProjectConfigs();
                            break;
                        }
                        catch (Exception ex)
                        {
                            exception = ex;
                        }
                    }

                    if (projectsConfigs == null)
                    {
                        //Log.Error(exception, "统一配置获取失败");
                        throw exception;
                    }

                    SetProjectsConfigs(projectsConfigs);
                }

                finally
                {
                    _manualResetEventSlim.Set();
                }
            });

            _luckWalnutSourceManager.Watching();
            _luckWalnutSourceManager.ProjectConfigSourceChanged += OnProjectConfigSourceChanged;
            _manualResetEventSlim.Wait();
            task.Wait();
        }

        
        /// <summary>
        /// 配置版本变更导致配置需要重新同步
        /// </summary>
        /// <returns></returns>
        private async Task OnProjectConfigSourceChanged()
        {
            var projectConfigs = await _luckWalnutSourceManager.GetProjectConfigs();
            SetProjectsConfigs(projectConfigs);
            try
            {
                if (ProjectConfigChanged is not null)
                {
                    ProjectConfigChanged.Invoke();    
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                // Log.Warn(ex, $"{appUK} ConfigChanged");
            }
        }

        public IDictionary<string, IDictionary<string, string>> GetNewProjectConfigs()
        {
            return _projectsConfigs;
        }
        public IDictionary<string, IDictionary<string, string>> GetConfig()
        {
            GetProjectConfigs();

            return _projectsConfigs;
        }
    }
}