using ICSharpCode.SharpZipLib.Zip;
using System.Collections.Generic;
using System.IO;
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
            await Task.Factory.StartNew(() =>
            {
                foreach (string file in pack_list)
                {
                    if (!File.Exists(App.handler.GameRootPath + @"\" + file))
                        continue;
                    if (file.Contains(".zip"))
                        try
                        {
                            string strDirectory = App.handler.GameRootPath + @"\";
                            ZipInputStream zip = new ZipInputStream(File.OpenRead(App.handler.GameRootPath + @"\" + file));
                            ZipEntry theEntry;

                            while ((theEntry = zip.GetNextEntry()) != null)
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
                            zip.Close();
                        }
                        catch
                        {
                            error++;
                        }
                        finally
                        {
                            File.Delete(App.handler.GameRootPath + @"\" + file);
                        }
                }
            });
            return error > 0 ? false : true;
        }
    }
}
