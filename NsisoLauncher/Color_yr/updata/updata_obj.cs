using System.Collections.Generic;

namespace NsisoLauncher.Color_yr.updata
{
    class updata_obj
    {
        /// <summary>
        /// 整合包名字
        /// </summary>
        public string packname { get; set; }
        /// <summary>
        /// 版本
        /// </summary>
        public string Vision { get; set; }
        /// <summary>
        /// mod列表
        /// </summary>
        public List<updata_mod> mods { get; set; }
    }

    class updata_mod
    { 
        /// <summary>
        /// mod名字
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 版本
        /// </summary>
        public string vision { get; set; }
        /// <summary>
        /// MD5
        /// </summary>
        public string check { get; set; }
        /// <summary>
        /// 下载地址
        /// </summary>
        public string url { get; set; }
        /// <summary>
        /// 本地地址
        /// </summary>
        public string local { get; set; }
    }
}
