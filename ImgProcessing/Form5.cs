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
using System.Numerics;
using Accord.Imaging;

namespace ImgProcessing
{
    public partial class Form5 : Form
    {
        public Form5()
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
            Bitmap temp = new Bitmap((Bitmap)pictureBox1.Image);
            Convert2GrayScaleFast(temp);
            pictureBox2.Image = temp;
        }

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

        private void button3_Click(object sender, EventArgs e)
        {
            Bitmap source = (Bitmap)pictureBox2.Image;

            Bitmap grayScaleBP = new Bitmap(2, 2, PixelFormat.Format8bppIndexed);
            Rectangle cloneRect = new Rectangle(0, 0, 256, 256);
            grayScaleBP = source.Clone(cloneRect, PixelFormat.Format8bppIndexed);
            ComplexImage complexImage = ComplexImage.FromBitmap(grayScaleBP);
            complexImage.ForwardFourierTransform();
            Bitmap fourierImage = complexImage.ToBitmap();
            pictureBox3.Image = fourierImage;
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Bitmap source = (Bitmap)pictureBox2.Image;
            Bitmap grayScaleBP = new Bitmap(2, 2, PixelFormat.Format8bppIndexed);
            Rectangle cloneRect = new Rectangle(0, 0, 512, 512);
            grayScaleBP = source.Clone(cloneRect, PixelFormat.Format8bppIndexed);
            ComplexImage complexImage = ComplexImage.FromBitmap(grayScaleBP);
            complexImage.BackwardFourierTransform();
            pictureBox4.Image = complexImage.ToBitmap();
        }

        //Apply High-Pass Filtering directly to Image(the image will be sharpener)(in my example best threshold around 200-210 so can be visible)
        private void button5_Click_1(object sender, EventArgs e)
        {
            pictureBox2.Image = HighPassFilter(pictureBox4.Image, Convert.ToInt32(textBox1.Text));
        }

        //Apply Low-Pass Filtering directly to Image(the image will be smoother)(threshold should be small)
        private void button6_Click(object sender, EventArgs e)
        {
            Bitmap source = (Bitmap)pictureBox4.Image;
            pictureBox2.Image = LowPassFilter(source,Convert.ToInt32(textBox2.Text));
        }


        //Step 4: Image in Frequency Domain
        public new static Bitmap Padding(Bitmap image)
        {
            int w = image.Width;
            int h = image.Height;
            int n = 0;
            
            //size of the output frequenced image need to be 2^n and check if it is over size of the image then break
            while (FFT.Size <= Math.Max(w, h))
            {
                FFT.Size = (int)Math.Pow(2, n);
                if (FFT.Size == Math.Max(w, h))
                {
                    break;
                }

                n++;
            }

            //Definition of sizing the padded image
            double horizontal_padding = FFT.Size - w;
            double vertical_padding = FFT.Size - h;
            int left_padding = (int)Math.Floor(horizontal_padding / 2);
            int top_padding = (int)Math.Floor(vertical_padding / 2);

            //Source image in the mode of "Read only"
            BitmapData image_data = image.LockBits(
                new Rectangle(0, 0, w, h),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            //Buffer array for saving datas of source image(image_data)
            int bytes = image_data.Stride * image_data.Height;
            byte[] buffer = new byte[bytes];
            Marshal.Copy(image_data.Scan0, buffer, 0, bytes);
            image.UnlockBits(image_data);

            //Result Image after padding
            Bitmap padded_image = new Bitmap(FFT.Size, FFT.Size);

            BitmapData padded_data = padded_image.LockBits(
                new Rectangle(0, 0, FFT.Size, FFT.Size),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb);

            //Result array for saving the padded pixels from source image
            int padded_bytes = padded_data.Stride * padded_data.Height;
            byte[] result = new byte[padded_bytes];

            for (int i = 3; i < padded_bytes; i += 4)
            {
                result[i] = 255;
            }

            //Basing on the sizes of the padded image(padded_data) and buffer array(source image) add it to result image
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int image_position = y * image_data.Stride + x * 4; 
                    int padding_position = y * padded_data.Stride + x * 4;
                    for (int i = 0; i < 3; i++)
                    {
                        result[padded_data.Stride * top_padding + 4 * left_padding + padding_position + i] =
                            buffer[image_position + i];
                    }
                }
            }

            Marshal.Copy(result, 0, padded_data.Scan0, padded_bytes);
            padded_image.UnlockBits(padded_data);

            return padded_image;
        }

        public class FFT
        {
            public static int Size { get; set; }

            //Convert pixel values into a complex map of number
            public static Complex[][] ToComplex(Bitmap image)
            {
                int w = image.Width;
                int h = image.Height;

                BitmapData input_data = image.LockBits(
                    new Rectangle(0, 0, w, h),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb);

                int bytes = input_data.Stride * input_data.Height;

                //buffer: array for saving pixel input values
                //result: a complex map of number for converting
                byte[] buffer = new byte[bytes];
                Complex[][] result = new Complex[w][];

                Marshal.Copy(input_data.Scan0, buffer, 0, bytes);
                image.UnlockBits(input_data);

                int pixel_position;

                //Proceed converting
                for (int x = 0; x < w; x++)
                {
                    result[x] = new Complex[h];
                    for (int y = 0; y < h; y++)
                    {
                        pixel_position = y * input_data.Stride + x * 4;
                        result[x][y] = new Complex(buffer[pixel_position], 0);
                    }
                }

                return result;
            }

            //Fourier transform function for taking map of complex numbers
            public static Complex[] Forward(Complex[] input, bool phaseShift = true)
            {
                var result = new Complex[input.Length];
                var omega = (float)(-2.0 * Math.PI / input.Length);

                if (input.Length == 1)
                {
                    result[0] = input[0];

                    //if (Complex.IsNaN(result[0]))
                    //{
                    //    return new[] { new Complex(0, 0) };
                    //}

                    return result;
                }

                var evenInput = new Complex[input.Length / 2];
                var oddInput = new Complex[input.Length / 2];

                for (int i = 0; i < input.Length / 2; i++)
                {
                    evenInput[i] = input[2 * i];
                    oddInput[i] = input[2 * i + 1];
                }

                var even = Forward(evenInput, phaseShift);
                var odd = Forward(oddInput, phaseShift);

                for (int k = 0; k < input.Length / 2; k++)
                {
                    int phase;

                    if (phaseShift)
                    {
                        phase = k - Size / 2;
                    }
                    else
                    {
                        phase = k;
                    }
                    //Creates a complex number from a point's polar coordinates.
                    //First parameter is the magnitude from the origin to the number
                    //Second number is the phase (angle from the line to the horizontal)
                    odd[k] *= Complex.FromPolarCoordinates(1, omega * phase);
                }

                //Get the result array
                for (int k = 0; k < input.Length / 2; k++)
                {
                    result[k] = even[k] + odd[k];
                    result[k + input.Length / 2] = even[k] - odd[k];
                }

                return result;
            }

            //Compute Fourier transformation on Image
            public static Complex[][] Forward(Bitmap image)
            {
                var p = new Complex[Size][];
                var f = new Complex[Size][];
                var t = new Complex[Size][];

                //complexImage: complex map of number from input pixels of image
                var complexImage = ToComplex(image);

                //Apply Fourier Transformation on the complex map (complexImage) (for the first size)
                for (int l = 0; l < Size; l++)
                {
                    p[l] = Forward(complexImage[l]);
                }

                //Apply Fourier Transformation for the second size
                for (int l = 0; l < Size; l++)
                {
                    t[l] = new Complex[Size];
                    for (int k = 0; k < Size; k++)
                    {
                        t[l][k] = p[k][l];
                    }

                    f[l] = Forward(t[l]);
                }

                return f;
            }

            //Compute the conjugation of the complex map of number(z=a-bi)
            public static Complex[] Inverse(Complex[] input)
            {
                for (int i = 0; i < input.Length; i++)
                {
                    input[i] = Complex.Conjugate(input[i]);
                }

                var transform = Forward(input, false);

                for (int i = 0; i < input.Length; i++)
                {
                    transform[i] = Complex.Conjugate(transform[i]);
                }

                return transform;
            }

            //Backward Fourier Transformation
            public static Bitmap Inverse(Complex[][] frequencies)
            {
                var p = new Complex[Size][];
                var f = new Complex[Size][];
                var t = new Complex[Size][];

                Bitmap image = new Bitmap(Size, Size);
                BitmapData image_data = image.LockBits(
                    new Rectangle(0, 0, Size, Size),
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format32bppArgb);
                int bytes = image_data.Stride * image_data.Height;
                byte[] result = new byte[bytes];

                //Compute the conjugation of the input complex number and save it into p[i]
                for (int i = 0; i < Size; i++)
                {
                    p[i] = Inverse(frequencies[i]);
                }

                //Proceeding on both sizes
                for (int i = 0; i < Size; i++)
                {
                    t[i] = new Complex[Size];
                    for (int j = 0; j < Size; j++)
                    {
                        t[i][j] = p[j][i] / (Size * Size);
                    }

                    f[i] = Inverse(t[i]);
                }

                //Apply the absolute of complex number f to array "result"(result image)
                for (int y = 0; y < Size; y++)
                {
                    for (int x = 0; x < Size; x++)
                    {
                        int pixel_position = y * image_data.Stride + x * 4;
                        for (int i = 0; i < 3; i++)
                        {
                            result[pixel_position + i] = (byte)Complex.Abs(f[x][y]);
                        }

                        result[pixel_position + 3] = 255;
                    }
                }

                Marshal.Copy(result, 0, image_data.Scan0, bytes);
                image.UnlockBits(image_data);
                return image;
            }

        }

        public Bitmap HighPassFilter(System.Drawing.Image input,int threshold)
        {
            Bitmap inputBMP = new Bitmap(input);
            Bitmap resultBMP = inputBMP;
            int prevPixel = 0;

            for(int y=0;y<input.Height;y++)
            {
                for(int x=0;x<input.Width;x++)
                {
                    int nowPixel = getPixelAverage(ref inputBMP, x, y);
                    if(Math.Abs(nowPixel-prevPixel)>threshold)
                    {
                        resultBMP.SetPixel(x, y, Color.Black);
                    }
                }
            }

            return resultBMP;
        }

        public Bitmap LowPassFilter(Bitmap input,int threshold)
        {
            Bitmap inputBMP = new Bitmap(input);
            Bitmap resultBMP = inputBMP;
            int prevPixel = 0;

            for (int y = 0; y < input.Height; y++)
            {
                for (int x = 0; x < input.Width; x++)
                {
                    int nowPixel = getPixelAverage(ref inputBMP, x, y);
                    if (Math.Abs(nowPixel - prevPixel) < threshold)
                    {
                        resultBMP.SetPixel(x, y,Color.DarkGray);
                    }
                }
            }

            return resultBMP;
        }

        public int getPixelAverage(ref Bitmap image,int x,int y)
        {
            Color pixel = image.GetPixel(x, y);
            return (pixel.R + pixel.G + pixel.B) / 3;
        }

    }
}
