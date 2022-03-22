using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImgProcessing
{
    public partial class Form2 : Form
    {
        public Form2()
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

        private void button10_Click(object sender, EventArgs e)
        {
            Histogram_Change();
            pictureBox10.Image = new Bitmap("D:\\Desktop\\file chứa photoshop\\Histogram.jpg");
        }
        public bool Histogram_Change()
        {
            using (var img1 = new Bitmap(pictureBox1.Image))
            {
                var w = img1.Width;
                var h = img1.Height;

                int[] N = new int[256];

                for (int i = 0; i < h; ++i)
                {
                    for (int j = 0; j < w; ++j)
                    {
                        var pix1 = img1.GetPixel(j, i);

                        int r1 = pix1.R;
                        int g1 = pix1.G;
                        int b1 = pix1.B;

                        //Tính toán độ sáng trung bình của 1 pixel theo các kênh
                        int c = (r1 + b1 + g1) / 3;
                        N[c]++;
                    }
                }
                int max = N.Max();
                double k = 1.0 * h / max;

                Bitmap img_out = new Bitmap(1000, 1000);
                using (Graphics g = Graphics.FromImage(img_out))
                {
                    for (int i = 0; i <= 255; i++)
                    {
                        // Draw line to screen.
                        g.DrawLine(Pens.Black, i, h - 1, i, h - 1 - (int)(N[i] * k));
                    }
                }

                img_out.Save("D:\\Desktop\\file chứa photoshop\\Histogram.jpg");
            }
            return true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Bitmap copyBitmap4 = new Bitmap((Bitmap)pictureBox1.Image);
            Gradation_Change(copyBitmap4);
            pictureBox2.Image = copyBitmap4;
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
                        double r_new = 255 * Math.Pow(r1 / 255, 2) * 0.5;
                        double g_new = 255 * Math.Pow(g1 / 255, 2) * 0.5;
                        double b_new = 255 * Math.Pow(b1 / 255, 2) * 0.5;

                        pix1 = Color.FromArgb(Convert.ToInt32(r_new), Convert.ToInt32(g_new), Convert.ToInt32(b_new));
                        img_out.SetPixel(j, i, pix1);
                    }
                }
            }
            return true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Draw_Lines();
        }
        public bool Draw_Lines() //Line y=(1/2)*(x^2)
        {
            Point[] p = {
               new Point(0, 0),   // start point of first spline
               new Point(10, 50),    // first control point of first spline
               new Point(20, 200),    // second control point of first spline

                new Point(30, 450),  // endpoint of first spline and
                                      // start point of second spline

                new Point(40, 800),   // first control point of second spline
                new Point(50, 1250),  // second control point of second spline
                new Point(60, 1800)};  // endpoint of second spline

            Bitmap img_out = new Bitmap(482, 399);

            using (Graphics g = Graphics.FromImage(img_out))
            {
                Pen pen = new Pen(Color.Blue);
                g.DrawLines(pen, p);
            }

            pictureBox3.Image = img_out;
            return true;
        }
    }
}
