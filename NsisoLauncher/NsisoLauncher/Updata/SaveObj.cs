using System.Collections.Generic;

namespace NsisoLauncher.Updata
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
        public Dictionary<string, UpdataItem> mods { get; set; }
        /// <summary>
        /// 魔改列表
        /// </summary>
        public List<UpdataItem> scripts { get; set; }
        /// <summary>
        /// 配置文件压缩包
        /// </summary>
        public List<UpdataItem> config { get; set; }
        /// <summary>
        /// 配置文件压缩包
        /// </summary>
        public List<UpdataItem> resourcepacks { get; set; }
    }

    public class UpdataItem
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
    class Mod_Obj
    {
        /// <summary>
        /// MOD id
        /// </summary>
        public string modid { get; set; }
        /// <summary>
        /// 名字
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// 版本
        /// </summary>
        public string version { get; set; }
        /// <summary>
        /// MC版本
        /// </summary>
        public string mcversion { get; set; }
        /// <summary>
        /// 网址
        /// </summary>
        public string url { get; set; }
        /// <summary>
        /// 更新网站
        /// </summary>
        public string updateUrl { get; set; }
        /// <summary>
        /// 作者列表
        /// </summary>
        public List<string> authorList { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string credits { get; set; }
        /// <summary>
        /// 日志文件
        /// </summary>
        public string logoFile { get; set; }
        /// <summary>
        /// 截图
        /// </summary>
        public List<string> screenshots { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public List<string> dependencies { get; set; }
    }
    class Mod_Obj_List
    {
        public class Root
        {
            public int modListVersion { get; set; }
            public List<Mod_Obj> modList { get; set; }
        }
    }
}