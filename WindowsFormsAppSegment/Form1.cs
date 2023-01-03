using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsAppSegment
{
    public partial class Form1 : Form
    {
        public static string fileName;
        public static string fileNameOut;

        public Form1()
        {
            InitializeComponent();
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            // открытие файла
            openFileDialog1.Filter = "All files (*.*)|*.*"; //(.bmp)|*.bmp|
            openFileDialog1.Title = "Open an Image File";
            openFileDialog1.FileName = "";

            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }

            fileName = openFileDialog1.FileName;

            // загрузка файла в pictureBox1
            Bitmap bmp = new Bitmap(openFileDialog1.FileName);
            pictureBox1.Image = bmp;

            // обработка изображения
            Recursive_connected_components(bmp);
        }
        //
        private void Recursive_connected_components(Bitmap B)
        {
            int width = B.Width;
            int height = B.Height;

            byte[] inputBytes = MyLoadBMP(B);
            sbyte[] outputBytes;
            outputBytes = Negate(inputBytes);
            int label = 0;
            Find_components(outputBytes, width, height, label);

            Bitmap bmpOut;
            if (SaveBMP(outputBytes, width, height) != "")
            {
                bmpOut = new Bitmap(fileNameOut);
                pictureBox2.Image = bmpOut;
            }
        }

        public static byte[] MyLoadBMP(Bitmap input)
        {
            BitmapData curBitmapData = input.LockBits(new Rectangle(0, 0, input.Width, input.Height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            int stride = curBitmapData.Stride;
            byte[] data = new byte[stride * input.Height];
            Marshal.Copy(curBitmapData.Scan0, data, 0, data.Length);
            input.UnlockBits(curBitmapData);
            byte[] outdata = new byte[input.Width * input.Height];

            for (int i = 0; i < input.Height; i++)
            {
                //String s = "";
                for (int j = 0; j < input.Width; j++)
                {
                    outdata[j + i * input.Width] = data[i * stride + j];
                    //s += outdata[j + i * input.Width] == 1 ? ' ' : '*';
                }
            }
            return outdata;
        }
        
        public static sbyte[] Negate(byte[] inb)
        {
            int L = inb.Length;
            sbyte[] outsb = new sbyte[inb.Length];

            for (int i = 0; i < L; i++)
            {
                if (inb[i] == 0)
                {
                    outsb[i] = -1;
                }
                else if (inb[i] == 1)
                {
                    outsb[i] = 15;
                }
            }

            return outsb;
        }

        public static void Find_components(sbyte[] LB, int width, int height, int label)
        {
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (LB[i * width + j] == -1)
                    {
                        label = (label + 1) % 16;

                        Search(LB, label, i, j, width, height);
                    }
                }
            }
        }

        public static int[,] Neighbors(int r, int c, int width, int height)
        {
            int[,] n = new int[4, 2] { { -1, -1 }, { -1, -1 }, { -1, -1 }, { -1, -1 } };
            кint[,] n = new int[4, 2] { { -1, -1 }, { -1, -1 }, { -1, -1 }, { -1, -1 } };

            if ((r > 0) && (c > 0) && (r < height) && (c < width))
            {
                //n = new int[4, 2] { { r - 1, c }, { r, c - 1 }, { r, c + 1 }, { r + 1, c } }; 

                n[0, 0] = r - 1;
                n[0, 1] = c;

                n[1, 0] = r;
                n[1, 1] = c - 1;

                n[2, 0] = r;
                n[2, 1] = c + 1;

                n[3, 0] = r + 1;
                n[3, 1] = c;
            }

            return n;
        }

        public static void Search(sbyte[] LB, int label, int r, int c, int width, int height)
        {
            LB[r * width + c] = Convert.ToSByte(label);
            int[,] Nset = Neighbors(r, c, width, height);

            for (int i = 0; i < Nset.GetLength(0); i++)
            {
                if (Nset[i, 0] >= 0 && LB[Nset[i, 0] * width + Nset[i, 1]] == -1)
                {
                    Search(LB, label, Nset[i, 0], Nset[i, 1], width, height);
                }
            }
            return;
        }

        private string SaveBMP(sbyte[] buffer, int width, int height)
        {
            //sbyte[] buffer, int width, int height
            /*
            for (int i = 0; i < height; i++)
            {
                String s = "";
                for (int j = 0; j < width; j++)
                    s += buffer[i * width + j] == 15 ? ' ' : Convert.ToChar((buffer[i * width + j] + 33) % 128);
                Console.WriteLine(s);
            }
            */            

            Bitmap b = new Bitmap(width, height, PixelFormat.Format8bppIndexed);

            Rectangle BoundsRect = new Rectangle(0, 0, width, height);
            BitmapData bmpData = b.LockBits(BoundsRect,
            ImageLockMode.WriteOnly,
            b.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            // add back dummy bytes between lines, make each line be a multiple of 4 bytes 
            int skipByte = bmpData.Stride - width;
            byte[] newBuff = new byte[buffer.Length + skipByte * height];
            for (int j = 0; j < height; j++)
            {
                Buffer.BlockCopy(buffer, j * (width), newBuff, j * (width + skipByte), width);
            }

            // fill in rgbValues 
            Marshal.Copy(newBuff, 0, ptr, newBuff.Length);
            b.UnlockBits(bmpData);

            // диалог сохранения файла
            saveFileDialog1.Title = "Save an Image File";
            saveFileDialog1.FileName = fileName.Replace(".", "Out.").Replace(".bmp", "");
            saveFileDialog1.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif";

            if (saveFileDialog1.ShowDialog() == DialogResult.Cancel)    // если отмена сохранения
            {
                return "";
            }

            fileNameOut = saveFileDialog1.FileName;

            // если имя файла не пусто, сохраняем выходное изображение в файл
            if (fileNameOut != "")
            {
                switch (saveFileDialog1.FilterIndex)
                {
                    case 1:
                        b.Save(fileNameOut, ImageFormat.Jpeg);
                        break;

                    case 2:
                        b.Save(fileNameOut, ImageFormat.Bmp);
                        break;

                    case 3:
                        b.Save(fileNameOut, ImageFormat.Gif);
                        break;
                }
            }

            return fileNameOut;
        }
    }
}
