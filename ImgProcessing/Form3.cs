using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Accord.Imaging.Filters;

namespace ImgProcessing
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        private Otsu ot = new Otsu();
        private Bitmap org;

        private Niblack nb = new Niblack();

        private Sauvola sv = new Sauvola();

        private Wulff wu = new Wulff();

        private Bradley_Roth br = new Bradley_Roth();



        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image = Bitmap.FromFile(openFile.FileName);
                org = (Bitmap)pictureBox1.Image.Clone();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Bitmap temp = (Bitmap)org.Clone();
            ot.Convert2GrayScaleFast(temp);
            int otsuThreshold = ot.getOtsuThreshold((Bitmap)temp);
            ot.threshold(temp, otsuThreshold);
            //textBox1.Text = otsuThreshold.ToString();
            pictureBox2.Image = temp;
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            //org = (Bitmap)pictureBox1.Image.Clone();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Bitmap copyBitmap_gavrilov = new Bitmap((Bitmap)pictureBox1.Image);
            Gavrivlov_method(copyBitmap_gavrilov);
            pictureBox3.Image =copyBitmap_gavrilov;
        }

        public bool Gavrivlov_method(Bitmap img_out)
        {
            var h = pictureBox1.Height;
            var w = pictureBox1.Width;

            var image3 = pictureBox1.Image as Bitmap;
            int t = 0;

            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var pix1 = image3.GetPixel(x, y);
                    t = t + ((pix1.R + pix1.G + pix1.B) / 3);
                    //sum of pixels
                }
            }

            t = t / (w * h);

            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var pix1 = image3.GetPixel(x, y);
                    if (((pix1.R + pix1.G + pix1.B) / 3) > t) { img_out.SetPixel(x, y, Color.White); }
                    else { img_out.SetPixel(x, y, Color.Black); }
                }
            }
            return true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Bitmap temp = (Bitmap)org.Clone();
            nb.Convert2GrayScaleFast(temp);
            nb.VariableThresholdingLocalProperties(temp, -0.2);
            pictureBox4.Image = temp;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Bitmap temp = (Bitmap)org.Clone();
            sv.Convert2GrayScaleFast(temp);
            sv.VariableThresholdingLocalProperties(temp, 0.35);
            pictureBox5.Image = temp;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Bitmap temp = (Bitmap)org.Clone();
            wu.Convert2GrayScaleFast(temp);
            wu.VariableThresholdingLocalProperties(temp);
            pictureBox6.Image = temp;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Bitmap temp = (Bitmap)org.Clone();
            br.Convert2GrayScaleFast(temp);
            br.BradleyAdaptiveThresholding(ref temp);
            pictureBox7.Image = temp;
        }
    }
}
