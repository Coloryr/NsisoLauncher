using NsisoLauncher.Config.ConfigObj;
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
        public async Task<ImageSource> GetHeadSculSource(UserNodeObj uuid, AuthenticationNodeObj args)
        {
            if (InjectorHead.uuid == uuid.SelectProfileUUID)
                return img ?? HeadUtils.bitmap;
            try
            {
                isload = true;
                string url = args.SkinUrl;
                if (args.SkinUrl.Contains("{UUID}"))
                {
                    url = url.Replace("{UUID}", uuid.SelectProfileUUID.Replace("-", ""));
                }
                else if (args.SkinUrl.Contains("{U-U-ID}"))
                {
                    url = url.Replace("{UUID}", uuid.SelectProfileUUID);
                }
                else if (args.SkinUrl.Contains("{NAME}"))
                {
                    url = url.Replace("{NAME}", uuid.UserName);
                }
                if (args.HeadType == HeadType.URL)
                    img = await new HeadUtils().GetByUrl(url);
                else
                    img = await new HeadUtils().GetByJson(url);
                InjectorHead.uuid = uuid.SelectProfileUUID;
                return img;
            }
            finally
            {
                isload = false;
            }
        }
    }
}
