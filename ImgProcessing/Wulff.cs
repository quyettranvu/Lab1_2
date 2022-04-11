﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImgProcessing
{
    class Wulff
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
        public void VariableThresholdingLocalProperties(Bitmap bmp)
        {
            int w = bmp.Width;
            int h = bmp.Height;

            BitmapData image_data = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            int bytes = image_data.Stride * image_data.Height;
            byte[] buffer = new byte[bytes];
            byte[] result = new byte[bytes];

            Marshal.Copy(image_data.Scan0, buffer, 0, bytes);
            bmp.UnlockBits(image_data);

            //Get global mean - this works only for grayscale images
            double mg = 0;
            double min_buffer = buffer[0];
            for (int i = 0; i < bytes; i += 3)
            {
                mg += buffer[i];
                if (buffer[i] < min_buffer) min_buffer = buffer[i];

            }
            mg /= (w * h);

            for (int x = 1; x < w - 1; x++)
            {
                for (int y = 1; y < h - 1; y++)
                {
                    int position = x * 3 + y * image_data.Stride;
                    double[] histogram = new double[256];

                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            int nposition = position + i * 3 + j * image_data.Stride;
                            histogram[buffer[nposition]]++;
                        }
                    }

                    histogram = histogram.Select(l => l / (w * h)).ToArray();

                    double mean = 0;
                    for (int i = 0; i < 256; i++)
                    {
                        mean += i * histogram[i];
                    }

                    double std = 0;
                    double max_std = 0;
                    for (int i = 0; i < 256; i++)
                    {
                        std += Math.Pow(i - mean, 2) * histogram[i];
                        if (std > max_std) max_std = std;
                    }
                    std = Math.Sqrt(std);
                    max_std=Math.Sqrt(max_std);

                    double threshold =(1-0.5)*mg+ 0.5 * min_buffer + 0.5*(std/max_std)*(mg-min_buffer);
                    for (int c = 0; c < 3; c++)
                    {
                        result[position + c] = (byte)((buffer[position] > threshold) ? 255 : 0);
                    }
                }
            }

            //Bitmap res_img = new Bitmap(w, h);
            BitmapData res_data = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            Marshal.Copy(result, 0, res_data.Scan0, bytes);
            bmp.UnlockBits(image_data);

            // return res_img;
        }
    }
}
