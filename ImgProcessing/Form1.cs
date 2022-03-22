using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace ImgProcessing
{
    public partial class Form1 : Form
    {
        public Form1()
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
            //tạo 1 đối tượng bitmap như hình ở pictureBox1, xử lý và gán nó vào pictureBox2.Image
            OpenFileDialog openFile = new OpenFileDialog();
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                pictureBox2.Image = new Bitmap(openFile.FileName);
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                pictureBox9.Image = new Bitmap(openFile.FileName);
            }
        }

        //Tạo hàm chuyển ảnh sang màu xám
        public bool ProcessImage(Bitmap bmp)
        {
            for(int i=0;i<bmp.Width; i++)
            {
                for(int j=0;j<bmp.Height;j++)
                {
                    //Lấy giá trị R,G,B của hình qua việc lấy pixel
                    Color bmpColor = bmp.GetPixel(i, j);
                    int red = bmpColor.R;
                    int green = bmpColor.G;
                    int blue = bmpColor.B;
                    int gray = (byte)(.299 * red + .587 * green + .114 * blue);
                    red = gray;
                    green = gray;
                    blue = gray;
                    bmp.SetPixel(i, j, Color.FromArgb(red, green, blue));
                }
            }

            return true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Bitmap copyBitmap = new Bitmap((Bitmap)pictureBox1.Image);
            ProcessImage(copyBitmap);
            pictureBox3.Image = copyBitmap;
        }


        private void button4_Click(object sender, EventArgs e)
        {
            Bitmap copyBitmap1 = new Bitmap((Bitmap)pictureBox1.Image);
            SumPixels(copyBitmap1);
            pictureBox4.Image = copyBitmap1;
        }

        public bool SumPixels(Bitmap img_out)
        {
            using (var img1 = new Bitmap(pictureBox1.Image))
            //Mở hai hình ảnh và thực hiện tổng pixel trên chúng
            using (var img = new Bitmap(pictureBox2.Image))
            {
                var w = img.Width;
                var h = img.Height;

                    for (int i = 0; i < h; ++i)
                    {
                        for (int j = 0; j < w; ++j)
                        {
                            var pix = img.GetPixel(j, i);

                            int r = pix.R;
                            int g = pix.G;
                            int b = pix.B;

                            var pix1 = img1.GetPixel(j, i);

                            int r1 = pix1.R;
                            int g1 = pix1.G;
                            int b1 = pix1.B;


                        int rmax = Clamp(r + r1, 0, 255);
                        int gmax = Clamp(g + g1, 0, 255);
                        int bmax = Clamp(b + b1, 0, 255);

                        pix = Color.FromArgb(rmax, gmax, bmax);
                            img_out.SetPixel(j, i, pix);
                        }
                    }
            }
          return true;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Bitmap copyBitmap2 = new Bitmap((Bitmap)pictureBox1.Image);
            AveragePixels(copyBitmap2);
            pictureBox5.Image = copyBitmap2;
        }

        public bool AveragePixels(Bitmap img_out)
        {
            using (var img1 = new Bitmap(pictureBox1.Image))
            //Mở hai hình ảnh và thực hiện tổng pixel trên chúng
            using (var img = new Bitmap(pictureBox2.Image))
            {
                var w = img.Width;
                var h = img.Height;

                for (int i = 0; i < h; ++i)
                {
                    for (int j = 0; j < w; ++j)
                    {
                        var pix = img.GetPixel(j, i);

                        int r = pix.R;
                        int g = pix.G;
                        int b = pix.B;

                        var pix1 = img1.GetPixel(j, i);

                        int r1 = pix1.R;
                        int g1 = pix1.G;
                        int b1 = pix1.B;

                        int rmax = Clamp((r + r1) / 2, 0, 255);
                        int gmax = Clamp((g + g1) / 2, 0, 255);
                        int bmax = Clamp((b + b1) / 2, 0, 255);

                        pix = Color.FromArgb(rmax, gmax, bmax);
                        img_out.SetPixel(j, i, pix);
                    }
                }
            }
            return true;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Bitmap copyBitmap3 = new Bitmap((Bitmap)pictureBox1.Image);
            MultiplyPixels(copyBitmap3);
            pictureBox6.Image = copyBitmap3;
        }

        public bool MultiplyPixels(Bitmap img_out)
        {
            using (var img1 = new Bitmap(pictureBox1.Image))
            //Mở hai hình ảnh và thực hiện tổng pixel trên chúng
            using (var img = new Bitmap(pictureBox2.Image))
            {
                var w = img.Width;
                var h = img.Height;

                for (int i = 0; i < h; ++i)
                {
                    for (int j = 0; j < w; ++j)
                    {
                        var pix = img.GetPixel(j, i);

                        int r = pix.R;
                        int g = pix.G;
                        int b = pix.B;

                        var pix1 = img1.GetPixel(j, i);

                        int r1 = pix1.R;
                        int g1 = pix1.G;
                        int b1 = pix1.B;

                        int rmax = Clamp(r * r1 / 255, 0, 255);
                        int gmax = Clamp(g * g1 / 255, 0, 255);
                        int bmax = Clamp(b * b1 / 255, 0, 255);

                        pix = Color.FromArgb(rmax, gmax, bmax);
                        img_out.SetPixel(j, i, pix);
                    }
                }
            }
            return true;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Mask_Image();
            pictureBox7.Image = new Bitmap("D:\\Desktop\\file chứa photoshop\\Composited.jpg");

        }

        public bool Mask_Image()
        {
            using (Image background = new Bitmap(pictureBox1.Image))
            using (Image masksource = new Bitmap(pictureBox9.Image))
            using (var imgattr = new ImageAttributes())
            {
                // set color key to Lime 
                imgattr.SetColorKey(Color.White, Color.White);

                // Draw non-lime portions of mask onto original
                using (var g = Graphics.FromImage(background))
                {
                    g.DrawImage(
                        masksource,
                        new Rectangle(0, 0, masksource.Width, masksource.Height),
                        0, 0, masksource.Width, masksource.Height,
                        GraphicsUnit.Pixel, imgattr
                    );
                }

                // Do something with the composited image here...
                background.Save("D:\\Desktop\\file chứa photoshop\\Composited.jpg");
            }
            return true;
        }


        private void button8_Click(object sender, EventArgs e)
        {
            Bitmap copyBitmap4 = new Bitmap((Bitmap)pictureBox1.Image);
            Gradation_Change(copyBitmap4);
            pictureBox8.Image = copyBitmap4;
        }

        public bool Gradation_Change(Bitmap img_out)
        {
            using (var img1 = new Bitmap(pictureBox1.Image))
            {
                var w = img1.Width;
                var h = img1.Height;

                for (int i = 0; i < h; ++i)
                {
                    for (int j = 0; j < w; ++j)
                    {
                        var pix1 = img1.GetPixel(j, i);

                        int r1 = pix1.R;
                        int g1 = pix1.G;
                        int b1 = pix1.B;

                        //giá trị tính được là ra giá trị double nên mình phải chuyển giá trị từ int sang double hoặc khai báo double
                        double r_new = 255 * Math.Pow(r1 / 255, 2);
                        double g_new = 255 * Math.Pow(g1 / 255, 2);
                        double b_new = 255 * Math.Pow(b1 / 255, 2);

                        pix1 = Color.FromArgb(Convert.ToInt32(r_new), Convert.ToInt32(g_new), Convert.ToInt32(b_new));
                        img_out.SetPixel(j, i, pix1);
                    }
                }
            }
            return true;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            //
        }
  

        public static T Clamp<T>(T val, T max, T min) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            //
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
