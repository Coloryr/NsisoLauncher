using ICSharpCode.SharpZipLib.Zip;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json.Linq;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Util.Installer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace NsisoLauncher.ModPack
{
    class CheckModPack
    {
        ProgressDialogController window;
        public CheckModPack(ProgressDialogController window)
        {
            this.window = window;
        }
        private DownloadTask GetForge(manifestObj obj)
        {
            string forge = obj.minecraft.modLoaders[0].id.Replace("forge", "");
            string local = App.Handler.GameRootPath + "\\forge-" + obj.minecraft.version + forge + ".jar";
            string forgePath = string.Format("maven/net/minecraftforge/forge/{0}{1}/forge-{0}{1}-installer.jar", obj.minecraft.version, forge);
            DownloadTask dt = new DownloadTask(App.GetResourceString("String.NewDownloadTaskWindow.Core.Forge"),
                string.Format("https://bmclapi2.bangbang93.com/{0}", forgePath),
                local);
            dt.Todo = new Func<Exception>(() =>
            {
                try
                {
                    CommonInstaller installer = new CommonInstaller(local, new CommonInstallOptions() 
                    { GameRootPath = App.Handler.GameRootPath });
                    installer.BeginInstall();
                    return null;
                }
                catch (Exception ex)
                { return ex; }
            });
            return dt;
        }

        public async Task<List<DownloadTask>> Check(string Local)
        {
            FileInfo info = new FileInfo(Local);
            List<DownloadTask> task = new List<DownloadTask>();
            if (info.Exists)
            {
                string strDirectory = App.Handler.GameRootPath + @"\";
                ZipInputStream zip = new ZipInputStream(File.OpenRead(Local));
                ZipEntry theEntry;

                while ((theEntry = zip.GetNextEntry()) != null)
                {
                    if (theEntry.Name == "manifest.json")
                    {
                        byte[] data = new byte[theEntry.Size];
                        zip.Read(data, 0, (int)theEntry.Size);
                        string a = Encoding.Default.GetString(data);
                        manifestObj obj = JObject.Parse(a).ToObject<manifestObj>();
                        GetUrl GetUrl = new GetUrl(window);

                        var res = await GetUrl.get_urlAsync(obj.files);
                        if (res == null)
                            return null;
                        foreach (var item in res)
                        {
                            task.Add(new DownloadTask("整合包mod", item.url, strDirectory + "mods\\" + item.filename));
                        }
                        task.Add(GetForge(obj));
                    }
                    else
                        try
                        {
                            string directoryName = "";
                            string pathToZip = theEntry.Name;
                            if (!string.IsNullOrWhiteSpace(pathToZip))
                                directoryName = Path.GetDirectoryName(pathToZip).Replace("overrides", "") + "\\";
                            string fileName = Path.GetFileName(pathToZip);
                            Directory.CreateDirectory(strDirectory + directoryName);
                            if (!string.IsNullOrWhiteSpace(fileName))
                            {
                                FileStream streamWriter = File.Create(strDirectory + directoryName + fileName);
                                int size = 1024 * 512;
                                byte[] data = new byte[1024 * 512];
                                while (size > 0)
                                {
                                    size = zip.Read(data, 0, data.Length);
                                    if (size > 0)
                                        streamWriter.Write(data, 0, size);
                                }
                                streamWriter.Close();
                            }
                        }
                        catch
                        {

                        }
                }
            }
            return task;
        }
    }
}
