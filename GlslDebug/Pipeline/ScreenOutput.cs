using System.Drawing;
using System.Numerics;
using System.Threading;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System;

namespace GlslDebug.Pipeline
{
    public class ScreenOutput
    {
        private static bool isShowing = false;
        private static ShowImage showImage = new ShowImage();
        private static Thread windowThread;
        

        public static void Output(Vector4[,] rgba)
        {
            var resultImage = new Bitmap(rgba.GetLength(0), rgba.GetLength(1), PixelFormat.Format32bppArgb);


            byte[] byteData = new byte[4 * rgba.GetLength(0) * rgba.GetLength(1)];

            for (int i = 0; i < rgba.GetLength(0); i++)
            {
                for (int k = rgba.GetLength(1) - 1; k >= 0; k--)
                {
                    int startIdx = i * rgba.GetLength(0) * 4 + k * 4;

                    //Data is read from back to front. Format is ARGB (WXYZ), thus data is written BGRA (ZYXW)
                    byteData[startIdx + 0] = (byte)(rgba[i, k].Z * 255); 
                    byteData[startIdx + 1] = (byte)(rgba[i, k].Y * 255);   
                    byteData[startIdx + 2] = (byte)(rgba[i, k].X * 255);
                    byteData[startIdx + 3] = (byte)(rgba[i, k].W * 255);
                }
            }


            //Thanks to: https://stackoverflow.com/questions/21555394/how-to-create-bitmap-from-byte-array
            unsafe
            {
                fixed (byte* ptr = byteData)
                {
                    using (Bitmap image = new Bitmap(rgba.GetLength(0), rgba.GetLength(1), rgba.GetLength(0) * 4, PixelFormat.Format32bppArgb, new IntPtr(ptr)))
                    {
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

                            showImage.UpdateImage(image);
                        }
                    }

                }
            }
        }
    }
}


/*
 * NOT USED CODE BUT MIGHT BE USEFUL
 * 
    --SAVE IMAGE DATA VIA STREAM
                            //MemoryStream stream = new MemoryStream();
                            //image.Save(stream, ImageFormat.Bmp);

                            //var data = stream.ToArray();

                            //MemoryStream stream2 = new MemoryStream();
                            //resultImage.Save(stream2, ImageFormat.Bmp);

                            //var data2 = stream2.ToArray();
    ---


    --SAVE IMAGE DATA WITHOUT HEADER VIA SCAN0
                            //BitmapData imageData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);
                            //BitmapData resultData = resultImage.LockBits(new Rectangle(0, 0, resultImage.Width, resultImage.Height), ImageLockMode.ReadOnly, resultImage.PixelFormat);

                            //byte[] imageDataArray = new byte[400 * 400 * 4];
                            //byte[] resultImageDataArray = new byte[400 * 400 * 4];
                            //Marshal.Copy(imageData.Scan0, imageDataArray, 0, imageDataArray.Length);
                            //Marshal.Copy(resultData.Scan0, resultImageDataArray, 0, resultImageDataArray.Length);


                            //Bitmap newImage = new Bitmap(rgba.GetLength(0), rgba.GetLength(1), rgba.GetLength(0) * 4, PixelFormat.Format32bppArgb, imageData.Scan0);

                            //image.UnlockBits(imageData);
                            //resultImage.UnlockBits(resultData);
    ---
    

    --EXTRACT HEADER FROM BITMAP
    

            //private static readonly int _bmpHeaderSize = 54;
            //private static readonly byte[] _bmpHeader = new byte[_bmpHeaderSize];


            //size + 54 bytes header
            //var headerImage = new Bitmap(rgba.GetLength(0), rgba.GetLength(1), PixelFormat.Format32bppArgb);
            //headerImage.SetPixel(0, 0, Color.FromArgb(200, 150, 100, 50));

            //MemoryStream headerStream = new MemoryStream();
            //headerImage.Save(headerStream, ImageFormat.Bmp);

            //var headerPlusData = headerStream.ToArray();

            //for (int i = 0; i < _bmpHeaderSize; i++)
            //{
            //    _bmpHeader[i] = headerPlusData[i];
            //}

            //byte[] byteData = new byte[4 * rgba.GetLength(0) * rgba.GetLength(1) + _bmpHeaderSize];

            //for(int i = 0; i < _bmpHeaderSize; i++)
            //{
            //    byteData[i] = _bmpHeader[i];
            //}
    ---
    */
