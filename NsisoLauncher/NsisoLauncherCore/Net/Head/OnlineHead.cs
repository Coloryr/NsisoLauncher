using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NsisoLauncherCore.Net.Head
{
    public class OnlineHead
    {
        public static bool isload = false;
        private static BitmapImage img;
        private static string uuid;
        public async Task<ImageSource> GetHeadSculSource(string uuid)
        {
            if (OnlineHead.uuid == uuid)
                return img ?? HeadUtils.bitmap;
            try
            {
                isload = true;
                string url = "https://sessionserver.mojang.com/session/minecraft/profile/" + uuid;
                img = await new HeadUtils().GetByJson(url);
                OnlineHead.uuid = uuid;
                return img;
            }
            finally
            {
                isload = false;
            }
        }
    }
}
