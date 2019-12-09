using ICSharpCode.SharpZipLib.Zip;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NsisoLauncher.Color_yr.updata
{
    class updata_pack
    {
        public List<string> pack_list = new List<string>();
        public async Task<bool> pack()
        {
            int error = 0;
            if (pack_list != null && pack_list.Count != 0)
            {
                foreach (string file in pack_list)
                {
                    if (File.Exists(App.handler.GameRootPath + @"\" + file))
                    {
                        try
                        {
                            string strDirectory = App.handler.GameRootPath + @"\";
                            ZipInputStream s = new ZipInputStream(File.OpenRead(App.handler.GameRootPath + @"\" + file));
                            ZipEntry theEntry;

                            while ((theEntry = s.GetNextEntry()) != null)
                            {
                                string directoryName = "";
                                string pathToZip = "";
                                pathToZip = theEntry.Name;
                                if (pathToZip != "")
                                    directoryName = Path.GetDirectoryName(pathToZip) + "\\";
                                string fileName = Path.GetFileName(pathToZip);
                                Directory.CreateDirectory(strDirectory + directoryName);
                                if (!string.IsNullOrWhiteSpace(fileName))
                                {
                                    if (File.Exists(strDirectory + directoryName + fileName) || (!File.Exists(strDirectory + directoryName + fileName)))
                                    {
                                        FileStream streamWriter = File.Create(strDirectory + directoryName + fileName);
                                        int size = 1024 * 1024;
                                        byte[] data = new byte[1024 * 1024];
                                        while (true)
                                        {
                                            size = s.Read(data, 0, data.Length);
                                            if (size > 0)
                                                streamWriter.Write(data, 0, size);
                                            else
                                                break;
                                        }
                                        streamWriter.Close();
                                    }
                                }
                            }
                            s.Close();
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
                }
                if (error > 0)
                    return false;
                else
                    return true;
            }
            return true;
        }
    }
}
