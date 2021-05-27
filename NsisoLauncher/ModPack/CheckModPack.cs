using ICSharpCode.SharpZipLib.Zip;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json.Linq;
using NsisoLauncherCore.Net;
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

        public async Task<List<DownloadTask>> Check(string Local)
        {
            FileInfo info = new FileInfo(Local);
            List<DownloadTask> task = new List<DownloadTask>();
            if (info.Exists)
            {
                string strDirectory = App.Handler.GameRootPath + @"\";
                ZipInputStream zip = new ZipInputStream(File.OpenRead(Local));
                ZipEntry theEntry;
                manifestObj obj = null;
                bool isfind = false;
                while ((theEntry = zip.GetNextEntry()) != null)
                {
                    if (isfind == false && theEntry.Name == "manifest.json")
                    {
                        if (theEntry.Size < 0)
                        {
                            theEntry.Size = 4096 * 1024;
                        }
                        byte[] data = new byte[theEntry.Size];
                        zip.Read(data, 0, (int)theEntry.Size);
                        string a = Encoding.Default.GetString(data);
                        obj = JObject.Parse(a).ToObject<manifestObj>();
                        GetUrl GetUrl = new GetUrl(window);

                        var res = await GetUrl.GeturlAsync(obj.files);
                        if (res == null)
                            return null;

                        foreach (var item in res)
                        {
                            task.Add(new DownloadTask(App.GetResourceString("String.NewDownloadTaskWindow.ModPack.Mod"),
                                item.url, strDirectory + "mods\\" + item.filename));
                        }
                        task.Add(new GetForge().Get(obj));
                        window.SetMessage(App.GetResourceString("String.NewDownloadTaskWindow.ModPack.UnZip"));
                        isfind = true;
                    }
                    else if (obj != null)
                        try
                        {
                            await Task.Factory.StartNew(() =>
                            {
                                string directoryName = "";
                                string pathToZip = theEntry.Name;
                                if (!string.IsNullOrWhiteSpace(pathToZip))
                                    directoryName = Path.GetDirectoryName(pathToZip).Replace(obj.overrides, "") + "\\";
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
                            });
                        }
                        catch (Exception e)
                        {
                            App.LogHandler.AppendFatal(e);
                        }
                }
            }
            return task;
        }
    }
}
