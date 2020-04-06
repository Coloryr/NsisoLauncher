using System;
using System.IO;
using System.Security.Cryptography;

namespace NsisoLauncherCore.Util.Checker
{
    public class MD5Checker : IChecker
    {
        public string CheckSum { get; set; }
        public string FilePath { get; set; }

        public bool CheckFilePass()
        {
            if (string.IsNullOrWhiteSpace(CheckSum))
            {
                throw new ArgumentException("检验器缺少校验值");
            }
            return string.Equals(CheckSum, GetFileChecksum(), StringComparison.OrdinalIgnoreCase);
        }

        public string GetFileChecksum()
        {
            if (string.IsNullOrWhiteSpace(FilePath))
            {
                throw new ArgumentException("检验器校验目标文件路径为空");
            }
            FileStream file = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
            try
            {
                MD5 md5 = new MD5CryptoServiceProvider();//创建SHA1对象
                byte[] md5Bytes = md5.ComputeHash(file);//Hash运算
                md5.Dispose();//释放当前实例使用的所有资源
                file.Dispose();
                string result = BitConverter.ToString(md5Bytes);//将运算结果转为string类型
                result = result.Replace("-", "");
                return result;
            }
            catch
            {
                throw new ArgumentException("校验器初始化失败，请尝试根据下面的步骤：\n" +
                    "1、单击“开始，运行”键入“gpedit.msc”，然后单击“确定”。\n" +
                    "2、依次展开“计算机配置”，展开“Windows设置”，展开“安全设置”，展开“本地策略”，然后单击“安全选项”\n" +
                    "3、在右窗格中, 双击“系统加密：”使用“FIPS 兼容的算法来加密，散列，和签名”，单击“禁用”，然后单击“确定”。");
            }
        }
    }
}
