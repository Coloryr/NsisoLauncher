using System.Collections.Generic;
using System.IO;

namespace NsisoLauncher.Utils
{
    class GetFiles
    {
        public List<string> getFileName(string path)
        {
            List<string> files = new List<string>();
            DirectoryInfo root = new DirectoryInfo(path);
            foreach (FileInfo f in root.GetFiles())
            {
                files.Add(f.FullName);
            }
            return files;
        }
        public List<string> getDirectory(string path)
        {
            List<string> files = new List<string>();
            files.AddRange(getFileName(path));
            DirectoryInfo root = new DirectoryInfo(path);
            foreach (DirectoryInfo d in root.GetDirectories())
            {
                files.AddRange(getDirectory(d.FullName));
            }
            return files;
        }
    }
}
