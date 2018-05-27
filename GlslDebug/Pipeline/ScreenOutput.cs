using System.Drawing;
using System.Numerics;
using System.Threading;
using System.Windows.Forms;

namespace GlslDebug.Pipeline
{
    public class ScreenOutput
    {
        private static bool isShowing = false;
        private static ShowImage showImage = new ShowImage();
        private static Thread windowThread;

        public static void Output(Vector4[,] rgba)
        {
            var resultImage = new Bitmap(rgba.GetLength(0), rgba.GetLength(1), System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            for (int i = 0; i < rgba.GetLength(0); i++)
            {
                for (int k = 0; k < rgba.GetLength(1); k++)
                {
                    resultImage.SetPixel(i, k, Color.FromArgb((int)(rgba[i, k].W * 255), (int)(rgba[i, k].X * 255), (int)(rgba[i, k].Y * 255), (int)(rgba[i, k].Z * 255)));
                }
            }
            if (!isShowing)
            {
                isShowing = true;
                windowThread = new Thread(delegate ()
                {
                    Application.EnableVisualStyles();
                    Application.Run(showImage);
                });

                windowThread.Start();
            }
            else
            {
                showImage.UpdateImage(resultImage);
            }
        }
    }
}
