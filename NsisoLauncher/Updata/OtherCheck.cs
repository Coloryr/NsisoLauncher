﻿using ICSharpCode.SharpZipLib.Zip;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NsisoLauncher.Updata
{
    class OtherCheck
    {
        public List<string> packList = new List<string>();
        public async Task<bool> pack()
        {
            int error = 0;
            if (packList?.Count == 0)
            {
                return true;
            }
            return await Task.Run(() =>
            {
                foreach (string file in packList)
                {
                    if (file.Contains(".zip"))
                        error += UnZip(file) ? 1 : 0;
                }
                return error > 0 ? false : true;
            });
        }
        private bool UnZip(string file)
        {
            try
            {
                string strDirectory = App.Handler.GameRootPath + @"\";
                using ZipInputStream zip = new(File.OpenRead(App.Handler.GameRootPath + @"\" + file));
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
                        using FileStream streamWriter = File.Create(strDirectory + directoryName + fileName);
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
                return true;
            }
            finally
            {
                //临时解决
                Task.Run(() =>
                {
                    del(file);
                }).Wait();
            }
            return false;
        }
        private void del(string file)
        {
            try
            {
                Thread.Sleep(1000);
                File.Delete(App.Handler.GameRootPath + @"\" + file);
            }
            catch
            {
                del(file);
            }
        }
    }
}
