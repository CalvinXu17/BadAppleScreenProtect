using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BadApple
{
    public partial class ScreenForm : Form
    {
        private string fpath = @"BadApple.Resource.imgs.zip";
        private Stream fstream;
        private StreamReader sr;

        private float fontsize = 5;
        private Font font;
        private SolidBrush brush;

        private Graphics g;
        private BufferedGraphics myBuffer;
        private Graphics bg;

        private Timer tVedio;
        private System.Media.SoundPlayer sound;
        private PointF fpoint;
        private bool playmusic = false;

        public ScreenForm()
        {
            InitializeComponent();
            Cursor.Hide(); // 隐藏光标
        }


        private void ScreenForm_Load(object sender, EventArgs e)
        {
            InitGraphics();
            InitPlayFiles();
        }

        /// <summary>
        /// 初始化GDI绘图参数
        /// </summary>
        private void InitGraphics()
        {
            g = CreateGraphics();
            brush = new SolidBrush(Color.White);

            // 开启双缓冲
            BufferedGraphicsContext currentContext = BufferedGraphicsManager.Current;
            myBuffer = currentContext.Allocate(g, this.DisplayRectangle);
            bg = myBuffer.Graphics;
            MyMeasureString(tstr); // 测量显示尺寸
        }

        /// <summary>
        /// 初始化播放文件
        /// </summary>
        private void InitPlayFiles()
        {
            HasMusic(); // 判断运行目录下是否有w音频文件

            // 从exe内嵌资源中获取文本文件流
            //Assembly asm = Assembly.GetExecutingAssembly();
            //fstream = asm.GetManifestResourceStream(fpath);
            //sr = new StreamReader(fstream, Encoding.UTF8);
            InitZipFile();

            if (playmusic)
            {
                sound = new System.Media.SoundPlayer();
                sound.SoundLocation = @"badapple.wav";
                sound.PlayLooping();
            }

            // 初始化定时器
            tVedio = new Timer();
            tVedio.Interval = 31;
            tVedio.Tick += PlayVideo;
            tVedio.Start();
            
        }

        /// <summary>
        /// 初始化Zip压缩文件读写参数(原文件30多M太大，压缩处理后只有1M)
        /// </summary>
        private void InitZipFile()
        {
            try
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                fstream = asm.GetManifestResourceStream(this.fpath);
                ZipArchive archive = new ZipArchive(fstream, ZipArchiveMode.Read);
                ZipArchiveEntry entry = archive.GetEntry("imgs.txt");
                sr = new StreamReader(entry.Open(), Encoding.UTF8);
            }
            catch
            {
                MessageBox.Show("文件初始化失败！！！");
                Application.Exit();
            }
        }

        /// <summary>
        /// 判断运行目录下是否有badapple.wav音频文件
        /// </summary>
        private void HasMusic()
        {
            playmusic = File.Exists("badapple.wav");
        }

        /// <summary>
        /// 重新开始播放
        /// </summary>
        private void RePlay()
        {
            tVedio.Stop();
            sr.Close();
            sr.Dispose();
            fstream.Close();
            fstream.Dispose();

            // 重新初始化zip读取
            InitZipFile();

            if (playmusic)
            {
                if(sound != null)
                {
                    sound.Stop();
                    sound.Play();
                }
            }
            tVedio.Start();
            GC.Collect(); // 手动释放一下内存
        }

        /// <summary>
        /// 计算刚好铺满屏幕时所需字体的大小
        /// </summary>
        private void MyMeasureString(string str)
        {
            font = new Font("Courier New", fontsize);
            SizeF size = bg.MeasureString(str, font);

            if (size.Width < this.Width && size.Height < this.Height)
            {
                while(size.Width < this.Width && size.Height < this.Height)
                {
                    fontsize++;
                    font = new Font("Courier New", fontsize);
                    size = bg.MeasureString(str, font);
                }
            }
            else
            {
                while (size.Width > this.Width || size.Height > this.Height)
                {
                    fontsize--;
                    font = new Font("Courier New", fontsize);
                    size = bg.MeasureString(str, font);
                }
            }
            if (fontsize < 5)
            {
                fontsize = 5;
                font = new Font("Courier New", fontsize);
                size = bg.MeasureString(str, font);
            }
            fpoint = new PointF((this.Width - size.Width) / 2f, 0);
        }

        /// <summary>
        /// 绘制（宽120个字符，长45个字符）
        /// </summary>
        private void PlayVideo(object sender, EventArgs e)
        {
            string line = string.Empty;
            bg.Clear(Color.Black);
            bool IsEnd = false; // 标记是否播放完
            
            for (int i = 0; i < 45; i++)
            {
                string tmp = sr.ReadLine();
                if (tmp == null) // null为播放完
                {
                    IsEnd = true;
                    break;
                }
                if (i != 44)
                    line += tmp + "\n";
            }
            if (!IsEnd)
            {
                bg.DrawString(line, font, brush, fpoint);
                myBuffer.Render();
            }
            else
            {
                RePlay(); // 播放完毕，重新播放
            }
        }

        /// <summary>
        /// 退出事件
        /// </summary>
        private void ScreenForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                tVedio.Stop();
                tVedio.Dispose();

                bg.Dispose();
                myBuffer.Dispose();
                g.Dispose();

                if (sr != null)
                {
                    sr.Close();
                    sr.Dispose();
                }
                if (fstream != null)
                {
                    fstream.Close();
                    fstream.Dispose();
                }

                font.Dispose();
                brush.Dispose();
                if (sound != null)
                    sound.Dispose();
            }
            catch { }
        }

        /// <summary>
        /// 监听鼠标事件
        /// </summary>
        private int mx;
        private bool mflag = false;
        private void ScreenForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (!mflag)
            {
                mx = e.X;
                mflag = true;
            }
            int nx = e.X;
            if (Math.Abs(nx - mx) > 0)
                Application.Exit();
        }

        /// <summary>
        /// 监听键盘按下
        /// </summary>
        private void ScreenForm_KeyDown(object sender, KeyEventArgs e)
        {
            Application.Exit();
        }

        private string tstr = @"@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                 @@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                 @@@@@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                       @@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                              @@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                                   @@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                                        @@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                                          @@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                                             @@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                                              @@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                                                @@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                                                  @@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                                                    @@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ @                                                   @@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                                                    @@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                                                    @@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                                                   @@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                                                   @@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                                              @@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                                                @@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                                                 @@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                                                 @@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ @@@@@@@@@@@                                               @@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@        @@@@@@                                             @@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@              @@@@@                                            @@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                  @@@@@                                           @@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                  @@@@@@                                           @@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                   @@@@@@                                           @@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                   @@@@@@@                                          @@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                  @@@@@@@@                                         @@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                    @@@@@@                                  @  @   @@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                    @@@@@                                  @@  @  @@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                   @@@@@                                 @@  @@  @@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                  @@                                  @ @@@ @@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                                                @    @@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                                                 @    @@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                                                   @@  @@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                                                   @@@ @@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                                                     @@@ @@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                                                     @@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                                                       @@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                                                       @@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                  @                                     @@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                @@@                                     @@@@@@@    @@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@               @@@@                                     @@@@       @@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@              @@@@@                                            @@@@@@@@@";

    }

}
