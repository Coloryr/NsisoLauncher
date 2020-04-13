using NsisoLauncher.Config;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NsisoLauncherCore.Net.Head
{
    class InjectorHead
    {
        public static bool isload = false;
        private static BitmapImage img;
        private static string uuid;
        public async Task<ImageSource> GetHeadSculSource(string uuid, AuthenticationNode args)
        {
            if (InjectorHead.uuid == uuid)
                return img ?? HeadUtils.bitmap;
            try
            {
                isload = true;
                string url = args.SkinUrl + "/"+ uuid;
                img = await new HeadUtils().GetByJsonAsync(url);
                InjectorHead.uuid = uuid;
                return img;
            }
            finally
            {
                isload = false;
            }
        }
    }
}
