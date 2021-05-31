using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json.Linq;
using NsisoLauncher.Utils;
using NsisoLauncherCore.Net;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NsisoLauncher.ModPack
{
    public record GetUrlRes
    {
        public string url { get; set; }
        public string filename { get; set; }
    }
    public record UrlResObj
    {
        public long id { get; set; }
        public string displayName { get; set; }
        public string fileName { get; set; }
        public string fileDate { get; set; }
        public long fileLength { get; set; }
        public int releaseType { get; set; }
        public int fileStatus { get; set; }
        public string downloadUrl { get; set; }
        public bool isAlternate { get; set; }
        public long alternateFileId { get; set; }
        public List<dependencieObj> dependencies { get; set; }
        public bool isAvailable { get; set; }
        public List<moduleObj> modules { get; set; }
        public long packageFingerprint { get; set; }
        public List<string> gameVersion { get; set; }
        public string installMetadata { get; set; }
        public string serverPackFileId { get; set; }
        public bool hasInstallScript { get; set; }
        public string gameVersionDateReleased { get; set; }
        public string gameVersionFlavor { get; set; }
    }
    public record dependencieObj
    {
        public long addonId { get; set; }
        public int type { get; set; }
    }
    public record moduleObj
    {
        public string foldername { get; set; }
        public long fingerprint { get; set; }
    }
    class GetUrl
    {
        ProgressDialogController window;
        public GetUrl(ProgressDialogController window)
        {
            this.window = window;
            window.Maximum = 1;
            window.Minimum = 0;
        }

        private string BaseUrl = @"https://addons-ecs.forgesvc.net/api/v2/addon/{0}/file/{1}";
        public async Task<List<GetUrlRes>> GeturlAsync(List<FilesItem> mods)
        {
            TaskbarManager.SetProgressState(TaskbarProgressBarState.Indeterminate);
            List<GetUrlRes> list = new();
            foreach (var item in mods)
            {
                window.SetMessage(App.GetResourceString("String.NewDownloadTaskWindow.ModPack.Now")
                    + list.Count + "/" + mods.Count);
                string res = await HttpRequesterAPI.HttpGetStringAsync(string.Format(BaseUrl, item.projectID, item.fileID));
                if (res == null)
                    return null;
                var obj1 = JObject.Parse(res).ToObject<UrlResObj>();
                list.Add(new GetUrlRes
                {
                    url = obj1.downloadUrl,
                    filename = obj1.fileName
                });
            }
            TaskbarManager.SetProgressState(TaskbarProgressBarState.NoProgress);
            return list;
        }
    }
}
