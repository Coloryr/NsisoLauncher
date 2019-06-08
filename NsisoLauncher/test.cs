using Microsoft.VisualStudio.DebuggerVisualizers;
using System;
using System.Drawing;
using System.Windows.Forms;

[assembly: System.Diagnostics.DebuggerVisualizer(
typeof(NsisoLauncher.ImageVisualizer),
typeof(VisualizerObjectSource),
Target = typeof(System.Drawing.Image),
Description = "Image Visualizer")]
namespace NsisoLauncher
{
    public class ImageVisualizer : DialogDebuggerVisualizer
    {
        override protected void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            Image image = (Image)objectProvider.GetObject();

            Form form = new Form();
            form.Text = string.Format("Width: {0}, Height: {1}", image.Width, image.Height);
            form.ClientSize = new Size(image.Width, image.Height);
            form.FormBorderStyle = FormBorderStyle.FixedToolWindow;

            PictureBox pictureBox = new PictureBox();
            pictureBox.Image = image;
            pictureBox.Parent = form;
            pictureBox.Dock = DockStyle.Fill;

            windowService.ShowDialog(form);
        }
    }
}
