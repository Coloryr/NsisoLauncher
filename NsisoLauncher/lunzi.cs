using NsisoLauncherCore.Net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NsisoLauncher.APIHandler_nide8
{
    class lunzi
    {

        public Bitmap BitmapImageToBitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);
                return new Bitmap(bitmap);
            }
        }
        public BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            Bitmap bitmapSource = new Bitmap(bitmap.Width, bitmap.Height);
            int i, j;
            for (i = 0; i < bitmap.Width; i++)
                for (j = 0; j < bitmap.Height; j++)
                {
                    System.Drawing.Color pixelColor = bitmap.GetPixel(i, j);
                    System.Drawing.Color newColor = System.Drawing.Color.FromArgb(pixelColor.R, pixelColor.G, pixelColor.B);
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
        public BitmapImage CaptureImage(BitmapImage fromImage)
        {
            //创建新图位图
            Bitmap bitmap = new Bitmap(8, 8);
            //创建作图区域
            Graphics graphic = Graphics.FromImage(bitmap);
            //矩形定义,将要在被截取的图像上要截取的图像区域的左顶点位置和截取的大小
            Rectangle rectSource = new Rectangle(8, 8, 8, 8);
            //矩形定义,将要把 截取的图像区域 绘制到初始化的位图的位置和大小
            //rectDest说明，将把截取的区域，从位图左顶点开始绘制，绘制截取的区域原来大小
            Rectangle rectDest = new Rectangle(0, 0, 8, 8);
            //截取原图相应区域写入作图区
            graphic.DrawImage(BitmapImageToBitmap(fromImage), rectDest, rectSource, GraphicsUnit.Pixel);
            //从作图区生成新图
            System.Drawing.Image saveImage = System.Drawing.Image.FromHbitmap(bitmap.GetHbitmap());

            Bitmap map = new Bitmap(saveImage);

            //释放资源   
            saveImage.Dispose();
            graphic.Dispose();
            bitmap.Dispose();

            return BitmapToBitmapImage(Zoom(map, 8));
        }
    }
    public class APIHandler_nide8
    {
        const string APIUrl = "sessionserver/session/minecraft/profile/";
        public async Task<ImageSource> GetHeadSculSource(string uuid)
        {
            string url = App.nide8Handler.BaseURL + APIUrl + uuid;
            try
            {
                var res = await APIRequester.HttpGetAsync(url);
                string b = await res.Content.ReadAsStringAsync();//把流转换为字符串并显示在文本框中
                if (res.IsSuccessStatusCode)
                {
                    int d = b.IndexOf("value");
                    b = b.PadLeft(d).Remove(0, d);
                    b = b.Replace("value\":\"", "");
                    b = b.Replace("\"}]}", "");
                    byte[] inArray = Convert.FromBase64String(b);
                    string c = Encoding.UTF8.GetString(inArray);
                    d = c.IndexOf("https://");
                    if (d != -1)
                    {
                        c = c.PadLeft(d).Remove(0, d);
                        c = c.Replace("\",\"metadata\":", "");
                        c = c.Replace("{\"model\":\"slim\"}}}}", "");
                        c = c.Replace("\"}}}", "");
                        res = await APIRequester.HttpGetAsync(c);
                        using (Stream stream = await res.Content.ReadAsStreamAsync())
                        {
                            var bImage = new BitmapImage();
                            bImage.BeginInit();
                            bImage.StreamSource = stream;
                            bImage.EndInit();
                            lunzi lunzi = new lunzi();
                            return lunzi.CaptureImage(bImage);
                        }
                    }
                }
            }
            catch
            {
                return new BitmapImage(new Uri("/NsisoLauncher;component/Resource/Steve.jpg"));
            }
            return new BitmapImage(new Uri("/NsisoLauncher;component/Resource/Steve.jpg"));
        }
    }
}
