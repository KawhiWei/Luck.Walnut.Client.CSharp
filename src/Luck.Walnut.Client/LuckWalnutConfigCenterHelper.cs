using Grpc.Net.Client;
using Luck.Walnut.V1;

namespace Luck.Walnut.Client
{
    public class LuckWalnutConfigCenterHelper
    {

        private ManualResetEventSlim _manualResetEventSlim;
        private string _appId;
        private string _environment;
        private IDictionary<string, IDictionary<string, string>> _projectsConfigs;
        private readonly ILuckWalnutSourceManager _luckWalnutSourceManager;

        public LuckWalnutConfigCenterHelper(string appId, string environment)
        {
            _manualResetEventSlim = new ManualResetEventSlim(false);
            _appId = appId;
            _environment = environment;
            _luckWalnutSourceManager = new LuckWalnutSourceManager(_appId, _environment);
            _projectsConfigs = new BlockingDictionary<string, IDictionary<string, string>>();
        }

        private void SetProjectsConfigs(IEnumerable<LuckWalnutConfigAdapter> luckWalnutConfigs)
        {
            var configDic = luckWalnutConfigs.ToDictionary(config=>config.Key,config=>config.Value);

            _projectsConfigs[_appId]=configDic;

        }




        private void GetProjectConfigs()
        {
            var task=Task.Factory.StartNew(async () =>
            {
                try
                {
                    Exception? exception = null;
                    IEnumerable<LuckWalnutConfigAdapter>? projectsConfigs = null;

                    for (int i = 0; i < 5; i++)  //尝试多次，防止单次运行出错
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


            _manualResetEventSlim.Wait();
            task.Wait();


            Console.WriteLine("asdnaskdkasdkasnkkdlasm");

        }

        public IDictionary<string, IDictionary<string, string>> GetConfig()
        {

            GetProjectConfigs();

            return _projectsConfigs;
        }

    }
}