using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NsisoLauncherCore.Net.Head
{
    public class OnlineHead
    {
        const string APIUrl = "https://crafatar.com/avatars/";
        bool overlay = true;
        public async Task<ImageSource> GetHeadSculSource(string uuid)
        {
            string arg = "?size=64";
            if (overlay)
            {
                arg += "&overlay";
            }
            string url = APIUrl + uuid + arg;
            var res = await APIRequester.HttpGetAsync(url);
            if (res.IsSuccessStatusCode)
            {
                using (Stream stream = await res.Content.ReadAsStreamAsync())
                {
                    var bImage = new BitmapImage();
                    bImage.BeginInit();
                    bImage.StreamSource = stream;
                    bImage.EndInit();
                    return bImage;
                }
            }
            else
            {
                return new BitmapImage(new Uri("pack://application:,,,/Resource/Steve.jpg"));
            }
        }
    }
}
