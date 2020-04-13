using NsisoLauncher.Config;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NsisoLauncherCore.Net.Head
{
    class Nide8Head
    {
        public static bool isload = false;
        private static BitmapImage img;
        private static string uuid;
        public async Task<ImageSource> GetHeadSculSource(string uuid, AuthenticationNode args)
        {
            if (Nide8Head.uuid == uuid)
                return img ?? HeadUtils.bitmap;
            try
            {
                isload = true;
                string url = "https://auth2.nide8.com:233/" + args.Property["nide8ID"] +
                    "/sessionserver/session/minecraft/profile/" + uuid;
                img = await new HeadUtils().GetByJsonAsync(url);
                Nide8Head.uuid = uuid;
                return img;
            }
            finally
            {
                isload = false;
            }
        }
    }
}
