using System.Collections.Generic;

namespace NsisoLauncher.ModPack
{
    public class manifestObj
    {
        public Minecraft minecraft { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string manifestType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int manifestVersion { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string version { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string author { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<FilesItem> files { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string overrides { get; set; }
    }
    public class ModLoadersItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string primary { get; set; }
    }

    public class Minecraft
    {
        /// <summary>
        /// 
        /// </summary>
        public string version { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<ModLoadersItem> modLoaders { get; set; }
    }

    public class FilesItem
    {
        /// <summary>
        /// 
        /// </summary>
        public int projectID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int fileID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string required { get; set; }
    }
}
