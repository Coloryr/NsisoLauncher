using System.Collections.Generic;

namespace NsisoLauncher.Color_yr.updata
{
    public class server_info
    {
        /// <summary>
        /// 更新的网址根目录
        /// </summary>
        public static string server_local { get; set; }
    }
    public class updata_obj
    {
        /// <summary>
        /// 整合包名字
        /// </summary>
        public string packname { get; set; }
        /// <summary>
        /// 版本
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// mod列表
        /// </summary>
        public Dictionary<string, updata_item> mods { get; set; }
        /// <summary>
        /// 魔改列表
        /// </summary>
        public List<updata_item> scripts { get; set; }
        /// <summary>
        /// 配置文件压缩包
        /// </summary>
        public List<updata_item> config { get; set; }
        /// <summary>
        /// 配置文件压缩包
        /// </summary>
        public List<updata_item> resourcepacks { get; set; }
    }

    public class updata_item
    {
        /// <summary>
        /// 资源类型
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// 资源名字
        /// </summary>
        public string name { get; set; }
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
        /// <summary>
        /// 文件名字
        /// </summary>
        public string filename { get; set; }
        /// <summary>
        /// 操作
        /// </summary>
        public string function { get; set; }
    }
}
