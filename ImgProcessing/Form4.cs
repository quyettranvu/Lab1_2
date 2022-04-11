using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImgProcessing
{
    public partial class Form4 : Form
    {
        public Form4()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image = new Bitmap(openFile.FileName);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Bitmap resultBitmap = new Bitmap((Bitmap)pictureBox1.Image);
            pictureBox2.Image = MedianFilter(resultBitmap, Convert.ToInt32(textBox1.Text)); //kích thước ma trận là 3x3
        }
        public static Bitmap MedianFilter(Bitmap sourceBitmap, int matrixSize)
        {
            //load source image in mode "Read Only"
            BitmapData sourceData =
                       sourceBitmap.LockBits(new Rectangle(0, 0,
                       sourceBitmap.Width, sourceBitmap.Height),
                       ImageLockMode.ReadOnly,
                       PixelFormat.Format32bppArgb);

            bool grayscale = false;

            //An array for saving pixel and for saving result
            byte[] pixelBuffer = new byte[sourceData.Stride *
                                          sourceData.Height];


            byte[] resultBuffer = new byte[sourceData.Stride *
                                           sourceData.Height];


            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0,
                                       pixelBuffer.Length);


            sourceBitmap.UnlockBits(sourceData);

            //Gray-scale make
            if (grayscale == true)
            {
                float rgb = 0;


                for (int k = 0; k < pixelBuffer.Length; k += 4)
                {
                    rgb = pixelBuffer[k] * 0.11f;
                    rgb += pixelBuffer[k + 1] * 0.59f;
                    rgb += pixelBuffer[k + 2] * 0.3f;


                    pixelBuffer[k] = (byte)rgb;
                    pixelBuffer[k + 1] = pixelBuffer[k];
                    pixelBuffer[k + 2] = pixelBuffer[k];
                    pixelBuffer[k + 3] = 255;
                }
            }

            /*filterOffset:index value for defining extra-matrix
              calcOffset:define amount of pixels in the extra-matrix
              byteOffset:define amout of pixels in the origin matrix
             */
            int filterOffset = (matrixSize - 1) / 2;
            int calcOffset = 0;

            
            int byteOffset = 0;

            //Check with the neighbour pixels to build the list of neighbour pixels and create the list of medians
            List<int> neighbourPixels = new List<int>();
            byte[] middlePixel;

            //Main algorithm
            for (int offsetY = filterOffset; offsetY <
                sourceBitmap.Height - filterOffset; offsetY++)
            {
                for (int offsetX = filterOffset; offsetX <
                    sourceBitmap.Width - filterOffset; offsetX++)
                {
                    byteOffset = offsetY *
                                 sourceData.Stride +
                                 offsetX * 4; //4 because of considering 4 components from pixelBuffer


                    neighbourPixels.Clear();

                    //pixel value beside (left,upper) at negative position compared to considered pixel
                    for (int filterY = -filterOffset;
                        filterY <= filterOffset; filterY++)
                    {
                        for (int filterX = -filterOffset;
                            filterX <= filterOffset; filterX++)
                        {


                            calcOffset = byteOffset +
                                         (filterX * 4) +
                                (filterY * sourceData.Stride);

                            //add integer value(median) as neighbour pixel
                            neighbourPixels.Add(BitConverter.ToInt32(
                                             pixelBuffer, calcOffset));
                        }
                    }


                    neighbourPixels.Sort();

                    //assigning the determined middle pixel to the current pixel as an array of pixel colour component bytes.
                    middlePixel = BitConverter.GetBytes(
                                       neighbourPixels[filterOffset]);


                    resultBuffer[byteOffset] = middlePixel[0];
                    resultBuffer[byteOffset + 1] = middlePixel[1];
                    resultBuffer[byteOffset + 2] = middlePixel[2];
                    resultBuffer[byteOffset + 3] = middlePixel[3];
                }
            }


            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width,
                                             sourceBitmap.Height);


            BitmapData resultData =
                       resultBitmap.LockBits(new Rectangle(0, 0,
                       resultBitmap.Width, resultBitmap.Height),
                       ImageLockMode.WriteOnly,
                       PixelFormat.Format32bppArgb);


            Marshal.Copy(resultBuffer, 0, resultData.Scan0,
                                       resultBuffer.Length);


            resultBitmap.UnlockBits(resultData);


            return resultBitmap;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Bitmap sourceBitmap = new Bitmap((Bitmap)pictureBox1.Image);
            pictureBox3.Image = ArithmeticMean(sourceBitmap);
        }

        //Arithmetic Mean Filter(Linear Filter)
        public static Bitmap ArithmeticMean(Bitmap image)
        {
            //Load source image in mode "Read-Only"
            int w = image.Width;
            int h = image.Height;
            BitmapData image_data = image.LockBits(
                new Rectangle(0, 0, w, h),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);

            //Array for saving pixels(buffer array)
            int bytes = image_data.Stride * image_data.Height;
            byte[] buffer = new byte[bytes];
            Marshal.Copy(image_data.Scan0, buffer, 0, bytes);
            image.UnlockBits(image_data);

            //value r for the size reduction(this filtering will change the size of the output image)
            //by changing the value of r we can get blur effect clearly
            int r = 2;
            int wres = w - 2 * r;
            int hres = h - 2 * r;

            //result image in mode "Write Only"
            Bitmap result_image = new Bitmap(wres, hres);
            BitmapData result_data = result_image.LockBits(
                new Rectangle(0, 0, wres, hres),
                ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);

            int res_bytes = result_data.Stride * result_data.Height;
            byte[] result = new byte[res_bytes];

            for (int x = r; x < w - r; x++)
            {
                for (int y = r; y < h - r; y++)
                {
                    int pixel_location = x * 3 + y * image_data.Stride;
                    int res_pixel_location = (x - r) * 3 + (y - r) * result_data.Stride;
                    double[] mean = new double[3];

                    for (int kx = -r; kx <= r; kx++)
                    {
                        for (int ky = -r; ky <= r; ky++)
                        {
                            int kernel_pixel = pixel_location + kx * 3 + ky * image_data.Stride;  //core pixel for linear reflecting

                            //sum all pixel values within the set(buffer) and divine by the size of the set
                            for (int c = 0; c < 3; c++)
                            {
                                mean[c] += buffer[kernel_pixel + c] / Math.Pow(2 * r + 2, 2); //plus 1 due to value of r
                            }
                        }
                    }

                    //convolution work:sum all the concided pixel values
                    for (int c = 0; c < 3; c++)
                    {
                        result[res_pixel_location + c] = (byte)mean[c];
                    }
                }
            }

            Marshal.Copy(result, 0, result_data.Scan0, res_bytes);
            result_image.UnlockBits(result_data);
            return result_image;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Bitmap sourceBitmap = new Bitmap((Bitmap)pictureBox1.Image);
            pictureBox2.Image = Convolve(sourceBitmap, GaussianBlur(Convert.ToInt32(textBox2.Text), Convert.ToInt32(textBox3.Text)));
        }

        //Gauss function (goal:reduce noises in image):weight-sigma,lenght:size of matrix
        public static double[,] GaussianBlur(int lenght, double weight)
        {
            double[,] kernel = new double[lenght, lenght];
            double kernelSum = 0;
            int foff = (lenght - 1) / 2;
            double distance = 0;
            double constant = 1d / (2 * Math.PI * weight * weight);
            for (int y = -foff; y <= foff; y++)
            {
                for (int x = -foff; x <= foff; x++)
                {
                    distance = ((y * y) + (x * x)) / (2 * weight * weight);
                    kernel[y + foff, x + foff] = constant * Math.Exp(-distance);
                    kernelSum += kernel[y + foff, x + foff];
                }
            }

            //the result outputs a kernel for convolving next step
            for (int y = 0; y < lenght; y++)
            {
                for (int x = 0; x < lenght; x++)
                {
                    kernel[y, x] = kernel[y, x] * 1d / kernelSum;
                }
            }
            return kernel;
        }

        //Image Convolution(for applying Gauss blur)
        public static Bitmap Convolve(Bitmap srcImage, double[,] kernel)
        {
            //source image in mode "Read-only"
            int width = srcImage.Width;
            int height = srcImage.Height;
            BitmapData srcData = srcImage.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            //Array for saving pixels: buffer array ; array for saving result: result array
            int bytes = srcData.Stride * srcData.Height;
            byte[] buffer = new byte[bytes];
            byte[] result = new byte[bytes];
            Marshal.Copy(srcData.Scan0, buffer, 0, bytes);
            srcImage.UnlockBits(srcData);

            int colorChannels = 3;
            double[] rgb = new double[colorChannels];
            int foff = (kernel.GetLength(0) - 1) / 2;
            int kcenter = 0;
            int kpixel = 0;
            for (int y = foff; y < height - foff; y++)
            {
                for (int x = foff; x < width - foff; x++)
                {
                    for (int c = 0; c < colorChannels; c++)
                    {
                        rgb[c] = 0.0;
                    }

                    //center pixel receives highest Gauss value
                    //the neighboring pixels receive the smaller values as the distance from the origin increases.
                    kcenter = y * srcData.Stride + x * 4;
                    for (int fy = -foff; fy <= foff; fy++)
                    {
                        for (int fx = -foff; fx <= foff; fx++)
                        {
                            kpixel = kcenter + fy * srcData.Stride + fx * 4;
                            for (int c = 0; c < colorChannels; c++)
                            {
                                rgb[c] += (double)(buffer[kpixel + c]) * kernel[fy + foff, fx + foff];
                            }
                        }
                    }

                    //If value of color pixel exceed (0,255) then return to 0 and 255, prospectively
                    for (int c = 0; c < colorChannels; c++)
                    {
                        if (rgb[c] > 255)
                        {
                            rgb[c] = 255;
                        }
                        else if (rgb[c] < 0)
                        {
                            rgb[c] = 0;
                        }
                    }
                    for (int c = 0; c < colorChannels; c++)
                    {
                        result[kcenter + c] = (byte)rgb[c];
                    }
                    result[kcenter + 3] = 255;
                }
            }

            //result image
            Bitmap resultImage = new Bitmap(width, height);
            BitmapData resultData = resultImage.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(result, 0, resultData.Scan0, bytes);
            resultImage.UnlockBits(resultData);
            return resultImage;
        }
    }
}
