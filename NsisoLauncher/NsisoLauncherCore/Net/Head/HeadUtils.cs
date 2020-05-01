using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace NsisoLauncherCore.Net.Head
{
    class HeadUtils
    {
        public static readonly BitmapImage bitmap = new BitmapImage(new Uri("pack://application:,,,/Resource/Steve.jpg"));
        //缩放
        public Bitmap Zoom(Bitmap orgimg, int times)
        {
            Bitmap newimg = new Bitmap(orgimg.Width * times, orgimg.Height * times);
            for (int i = 0; i < orgimg.Width; i++)
            {
                for (int j = 0; j < orgimg.Height; j++)
                {
                    for (int x = i * times; x < (i + 1) * times; x++)
                    {
                        for (int y = j * times; y < (j + 1) * times; y++)
                        {
                            newimg.SetPixel(x, y, orgimg.GetPixel(i, j));
                        }
                    }
                }
            }
            return newimg;
        }
        public BitmapImage CaptureImage(Image fromImage)
        {
            //创建新图位图
            Bitmap bitmap = new Bitmap(8, 8);
            Bitmap bitmap_top = new Bitmap(8, 8);
            bitmap_top.MakeTransparent();
            //创建作图区域
            Graphics graphic = Graphics.FromImage(bitmap);
            Graphics graphic_top = Graphics.FromImage(bitmap_top);
            //矩形定义,将要在被截取的图像上要截取的图像区域的左顶点位置和截取的大小
            Rectangle rectSource = new Rectangle(8, 8, 8, 8);
            Rectangle rectSource_top = new Rectangle(40, 8, 8, 8);
            //矩形定义,将要把 截取的图像区域 绘制到初始化的位图的位置和大小
            //rectDest说明，将把截取的区域，从位图左顶点开始绘制，绘制截取的区域原来大小
            Rectangle rectDest = new Rectangle(0, 0, 8, 8);
            //截取原图相应区域写入作图区
            graphic.DrawImage(fromImage, rectDest, rectSource, GraphicsUnit.Pixel);
            graphic_top.DrawImage(fromImage, rectDest, rectSource_top, GraphicsUnit.Pixel);
            graphic.DrawImage(bitmap_top, rectDest, rectDest, GraphicsUnit.Pixel);
            Bitmap save = new Bitmap(bitmap);
            //释放资源   
            graphic.Dispose();
            graphic_top.Dispose();
            bitmap.Dispose();
            bitmap_top.Dispose();
            return BitmapToBitmapImage(Zoom(save, 8));
        }

        public BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            Bitmap bitmapSource = new Bitmap(bitmap.Width, bitmap.Height);
            int i, j;
            for (i = 0; i < bitmap.Width; i++)
                for (j = 0; j < bitmap.Height; j++)
                {
                    Color pixelColor = bitmap.GetPixel(i, j);
                    Color newColor = Color.FromArgb(pixelColor.R, pixelColor.G, pixelColor.B);
                    bitmapSource.SetPixel(i, j, newColor);
                }
            MemoryStream ms = new MemoryStream();
            bitmapSource.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(ms.ToArray());
            bitmapImage.EndInit();

            return bitmapImage;
        }
        public async Task<BitmapImage> GetByJson(string url)
        {
            var http = new HttpRequesterAPI(TimeSpan.FromSeconds(10));
            string data = await http.HttpGetStringAsync(url);
            if (data == null || data.Contains("error"))
            {
                return bitmap;
            }
            var obj1 = JObject.Parse(data);
            var properties = obj1["properties"];
            byte[] inArray = Convert.FromBase64String(properties[0]["value"].ToString());
            string c = Encoding.UTF8.GetString(inArray);
            if (c != "")
            {
                var obj2 = JObject.Parse(c);
                var textures = obj2["textures"]["SKIN"]["url"].ToString();
                HttpResponseMessage temp = await http.HttpGetAsync(textures);
                var img = Image.FromStream(await temp.Content.ReadAsStreamAsync());
                return CaptureImage(img);
            }
            return bitmap;
        }
        public async Task<BitmapImage> GetByUrl(string url)
        {
            var http = new HttpRequesterAPI(TimeSpan.FromSeconds(10));
            HttpResponseMessage temp = await http.HttpGetAsync(url);
            if (temp != null)
            {
                var img = Image.FromStream(await temp.Content.ReadAsStreamAsync());
                return CaptureImage(img);
            }
            return bitmap;
        }
    }
}
