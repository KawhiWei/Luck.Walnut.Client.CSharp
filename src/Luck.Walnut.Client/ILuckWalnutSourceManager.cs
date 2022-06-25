namespace Luck.Walnut.Client
{
    public interface ILuckWalnutSourceManager
    {
        Task<ProjectConfigAdapter> GetProjectConfigs();

        /// <summary>
        /// ws委托回调事件
        /// </summary>
        event Func<Task>? ProjectConfigSourceChanged;

        void Watching();
    }
}
