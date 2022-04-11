using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImgProcessing
{
    class Bradley_Roth
    {
        // simple routine to convert to gray scale
        public void Convert2GrayScaleFast(Bitmap bmp)
        {
            BitmapData bmData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                    ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            unsafe
            {
                byte* p = (byte*)(void*)bmData.Scan0.ToPointer();
                int stopAddress = (int)p + bmData.Stride * bmData.Height;
                while ((int)p != stopAddress)
                {
                    p[0] = (byte)(.299 * p[2] + .587 * p[1] + .114 * p[0]);
                    p[1] = p[0];
                    p[2] = p[0];
                    p += 3;
                }
            }
            bmp.UnlockBits(bmData);
        }

        private static byte[] BufferFromImage(ref Bitmap bitmap)
        {
            // Lock the bitmap's bits.  
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);

            byte[] grayValues = null;

            try
            {
                // Get the address of the first line.
                IntPtr ptr = bmpData.Scan0;

                // Declare an array to hold the bytes of the bitmap.
                int bytes = bmpData.Width * bitmap.Height;
                grayValues = new byte[bytes];

                // Copy the gray values into the array.
                Marshal.Copy(ptr, grayValues, 0, bytes);
            }
            finally
            {
                bitmap.UnlockBits(bmpData);
            }

            return grayValues;

            //using (MemoryStream memoryStream = new MemoryStream())
            //{
            //	bitmap.Save(memoryStream, ImageFormat.Bmp);
            //	buffer = memoryStream.ToArray();
            //}
        }

        public static void ImageFromBuffer(byte[] bytes, int w, int h, int ch, ref Bitmap bitmap)
        {
            //bitmap = new Bitmap(w, h, PixelFormat.Format8bppIndexed);
            BitmapData bmData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            try
            {
                IntPtr pNative = bmData.Scan0;
                Marshal.Copy(bytes, 0, pNative, h * ch);
            }
            finally
            {
                bitmap.UnlockBits(bmData);
            }

            //return bitmap;
            // using (var ms = new MemoryStream(imageData)) { bitm = new Bitmap(ms); }
        }

        private static int Idx2dTo1d(int i, int j, int matrixWidth)
        {
            return matrixWidth * j + i;
        }

        private static void ValidateSIndexesX(ref int x1, ref int x2, int w)
        {
            if (x1 < 0)
                x1 = 0;
            else if (x2 >= w)
                x2 = w - 1;
        }

        private static void ValidateSIndexesY(ref int y1, ref int y2, int h)
        {
            if (y1 < 0)
                y1 = 0;
            else if (y2 >= h)
                y2 = h - 1;
        }

        public void BradleyAdaptiveThresholding(ref Bitmap bitmap)
        {
            var w = bitmap.Width;
            var h = bitmap.Height;

            var input = BufferFromImage(ref bitmap);
            var output = new byte[input.LongLength];
            var intImg = new long[input.LongLength]; //Integral Images
            long sum = 0;
            //int s = w / 14; //s x s sub-matrix;
            int halfS = 1;
            float t = 0.85f; 

            // Create the integral image
            for (int i = 0; i < w; i++)
            {
                sum = 0;
                for (int j = 0; j < h; j++)
                {
                    int idx1d = Idx2dTo1d(i, j, w); //calc 1-dimensional index

                    sum = sum + input[idx1d];
                    if (i == 0)
                        intImg[idx1d] = sum;
                    else
                        intImg[idx1d] = intImg[Idx2dTo1d(i - 1, j, w)] + sum;
                }
            }

            // Perform thresholding
            int x1, x2, y1, y2;
            for (int i = 0; i < w; i++)
            {
                x1 = i - halfS;
                x2 = i + halfS;
                ValidateSIndexesX(ref x1, ref x2, w);

                for (int j = 0; j < h; j++)
                {
                    y1 = j - halfS;
                    y2 = j + halfS;
                    ValidateSIndexesY(ref y1, ref y2, h);

                    int count = (x2 - x1) * (y2 - y1);

                    int x1M1 = Math.Abs(x1 - 1);
                    int y1M1 = Math.Abs(y1 - 1);

                    sum = intImg[Idx2dTo1d(x2, y2, w)]
                        - intImg[Idx2dTo1d(x2, y1M1, w)]
                        - intImg[Idx2dTo1d(x1M1, y2, w)]
                        + intImg[Idx2dTo1d(x1M1, y1M1, w)];

                    int idx1d = Idx2dTo1d(i, j, w); //calc 1-dimensional index

                    if (input[idx1d] * count <= sum * t)
                        output[idx1d] = 0;
                    else
                        output[idx1d] = 255;
                }
            }

            ImageFromBuffer(output, w, h, 1, ref bitmap);
        }
    }
}
