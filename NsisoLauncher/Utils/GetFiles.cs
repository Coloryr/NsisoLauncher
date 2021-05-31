using System.Collections.Generic;
using System.IO;

namespace NsisoLauncher.Utils
{
    class GetFiles
    {
        public List<string> GetFileName(string path)
        {
            List<string> files = new();
            DirectoryInfo root = new(path);
            foreach (FileInfo f in root.GetFiles())
            {
                files.Add(f.FullName);
            }
            return files;
        }
        public List<string> GetDirectory(string path)
        {
            List<string> files = new();
            files.AddRange(GetFileName(path));
            DirectoryInfo root = new(path);
            foreach (DirectoryInfo d in root.GetDirectories())
            {
                files.AddRange(GetDirectory(d.FullName));
            }
            return files;
        }
    }
}
