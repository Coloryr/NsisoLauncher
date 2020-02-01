using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NsisoLauncher.Updata
{
    class OtherCheck
    {
        public List<string> pack_list = new List<string>();
        public async Task<bool> pack()
        {
            int error = 0;
            if (pack_list?.Count == 0)
            {
                return true;
            }
            return await Task.Factory.StartNew(() =>
            {
                foreach (string file in pack_list)
                {
                    if (file.Contains(".zip"))
                        try
                        {
                            string strDirectory = App.Handler.GameRootPath + @"\";
                            ZipInputStream zip = new ZipInputStream(File.OpenRead(App.Handler.GameRootPath + @"\" + file));
                            ZipEntry theEntry;

                            while ((theEntry = zip.GetNextEntry()) != null)
                            {
                                try
                                {
                                    string directoryName = "";
                                    string pathToZip = theEntry.Name;
                                    if (!string.IsNullOrWhiteSpace(pathToZip))
                                        directoryName = Path.GetDirectoryName(pathToZip) + "\\";
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
                            zip.Close();
                        }
                        catch
                        {
                            error++;
                        }
                        finally
                        {
                            //临时解决
                            Task.Factory.StartNew(() =>
                            {
                            a:
                                try
                                {
                                    Thread.Sleep(1000);
                                    File.Delete(App.Handler.GameRootPath + @"\" + file);
                                }
                                catch
                                {
                                    goto a;
                                }
                            });
                        }
                }
                return error > 0 ? false : true;
            });
        }
    }
}
