using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Collections;
using System.Runtime.InteropServices;

namespace MainForm
{
    public partial class Form1 : Form
    {
        Dictionary<int, RotateFlipType> rotateMap = new Dictionary<int, RotateFlipType> {
            {0,RotateFlipType.RotateNoneFlipX },{1,RotateFlipType.Rotate180FlipX},{2,RotateFlipType.Rotate90FlipX},
            {3,RotateFlipType.Rotate90FlipY }
        };//旋转字典，根据方向旋转图片
        List<Rectangle> makeTank = new List<Rectangle>();//坦克出现的位置集合，避免同时出现在一个位置
        bool fresh = true;//地图是否刷新
        //墙地图位置
        List<Point> wallMapList = new List<Point>() { new Point(200,440), new Point(200, 460), new Point(200, 480),
        new Point(200, 500),new Point(200, 520),new Point(200, 540),new Point(200, 560),new Point(220, 440),new Point(220, 460),
        new Point(220, 480),new Point(220, 500),new Point(220, 520),new Point(220, 540),new Point(220, 560),new Point(580, 440),
        new Point(580, 460),new Point(580, 480),new Point(580, 500),new Point(580, 520),new Point(580, 540),new Point(580, 560),
        new Point(600, 440),new Point(600, 460),new Point(600, 480),new Point(600, 500),new Point(600, 520),new Point(600, 540),
        new Point(600, 560),new Point(200, 100),new Point(200, 120),new Point(200, 140),new Point(200, 160),new Point(200, 180),
        new Point(200, 200),new Point(200, 220),new Point(220, 100),new Point(220, 120),new Point(220, 140),new Point(220, 160),
        new Point(220, 180),new Point(220, 200),new Point(220, 220),new Point(580, 100),new Point(580, 120),new Point(580, 140),
        new Point(580, 160),new Point(580, 180),new Point(580, 200),new Point(580, 220),new Point(600, 100),new Point(600, 120),
        new Point(600, 140),new Point(600, 160),new Point(600, 180),new Point(600, 200),new Point(600, 220),
        new Point(160,350),new Point(180,350),new Point(200,350),new Point(220,350),new Point(240,350),new Point(260,350),
        new Point(160,370),new Point(180,370),new Point(200,370),new Point(220,370),new Point(240,370),new Point(260,370),
        new Point(540,350),new Point(560,350),new Point(580,350),new Point(600,350),new Point(620,350),new Point(640,350),
        new Point(540,370),new Point(560,370),new Point(580,370),new Point(600,370),new Point(620,370),new Point(640,370),
        new Point(775,520), new Point(795,520), new Point(815,520),new Point(775,540), new Point(795,540), new Point(815,540),
        new Point(715,520), new Point(735,520), new Point(755,520),new Point(715,540), new Point(735,540), new Point(755,540)};



        public Form1()
        {
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景.
            SetStyle(ControlStyles.DoubleBuffer, true); // 双缓冲
            addWall();
        }

        Controller controller = new Controller();
        private void Form1_Load(object sender, EventArgs e)
        {
            this.KeyPreview = true;
            //玩家控制坦克移动
            Thread playerRunThread = new Thread(new ThreadStart(controller.playerRun));
            playerRunThread.Start();

            //控制敌方坦克移动
            Thread enemyRunThread = new Thread(new ThreadStart(controller.enemyRuns));
            enemyRunThread.Start();

            Thread invalidateThread = new Thread(new ThreadStart(timeInvalidate));
            invalidateThread.Start();
        }

        public void timeInvalidate()
        {
            while (controller.gameRun)
            {
                Invalidate();
                Thread.Sleep(10);
            }
        }


        //键盘事件
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                    controller.playerTank.IsLaunch = false;
                    controller.upPress = true;
                    break;
                case Keys.Down:
                    controller.playerTank.IsLaunch = false;
                    controller.downPress = true;
                    break;
                case Keys.Left:
                    controller.playerTank.IsLaunch = false;
                    controller.leftPress = true;
                    break;
                case Keys.Right:
                    controller.playerTank.IsLaunch = false;
                    controller.rightPress = true;
                    break;
                case Keys.Space:
                    if (controller.hitCount == 4)
                    {
                        Thread.Sleep(10000);
                    }
                    if (controller.playerBulletIsOver)
                    {
                        Thread bulletThread = new Thread(new ParameterizedThreadStart(controller.playerBulletRun));//创建一个线程
                        bulletThread.Start((object)controller.playerTank);                  //开启线程
                        Thread.Sleep(10);
                    }
                    break;
                default:
                    break;
            }

        }

        //绘图事件
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Image tankeImg = Image.FromFile("Img/tank1.png");
            tankeImg.RotateFlip(controller.rotate);
            g.DrawImage(tankeImg, controller.playerTank.Rectangle);
            Image playerBulletImage = Image.FromFile("Img/bullet1.png");
            for (int k = 0; k < controller.playerBulletList.Count; k++)
            {
                if (k < controller.playerBulletList.Count)
                {
                    try
                    {
                        playerBulletImage.RotateFlip(rotateMap[controller.playerBulletList[k].Direction]);
                        g.DrawImage(playerBulletImage, controller.playerBulletList[k].Rectangle);
                    }
                    catch (Exception ex)
                    {                    }
                }
            }
            
                drawMap(e);
            
            //敌方坦克
            Random random = new Random();
            for (int i = 0; i < 4; i++)
            {
                int x, y;
                Rectangle rec;
                do
                {
                    x = random.Next(0, 600);
                    y = random.Next(0, 100);
                    rec = new Rectangle(x, y, 34, 34);
                } while (controller.makeTankHit(rec, makeTank) || controller.wallTankHit(rec, controller.wallList));
                if (controller.enemyTank.Count < 4)
                {
                    makeTank.Add(rec);
                }
                int dirc = random.Next(0, 4);
                Tank tank = new Tank(x, y, 1, dirc);
                tank.Num = i;
                if (controller.enemyTank.Count < 4)
                {
                    controller.enemyTank.Add(tank);
                }
                Image tankImg = Image.FromFile("Img/enemytank.png");
                tankImg.RotateFlip(rotateMap[controller.enemyTank[i].Direction]);
                g.DrawImage(tankImg, controller.enemyTank[i].Rectangle);
            }
            for (int j = 0; j < controller.enemyBulletList.Count; j++)
            {
                if (j < controller.enemyBulletList.Count)
                {
                    try   //由于线程执行顺序的不确定性，移除子弹的时候长度减小，会越界，较难解决，先用异常处理
                    {
                        Image bulletImage = Image.FromFile("Img/bullet1.png");
                        bulletImage.RotateFlip(rotateMap[controller.enemyBulletList[j].Direction]);
                        g.DrawImage(bulletImage, controller.enemyBulletList[j].Rectangle);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }

            //单独画草，能刷新并且避免卡顿现象
            Grass grass1 = new Grass(0, 0,150,50);
            Image grassImg = Image.FromFile("Img/grass.png");
            g.DrawImage(grassImg, grass1.Rectangle);
            Grass grass2 = new Grass(330, 460,150,50);
            g.DrawImage(grassImg, grass2.Rectangle);
            Grass grass3 = new Grass(685, 160, 150, 50);
            g.DrawImage(grassImg, grass3.Rectangle);
            Grass grass4 = new Grass(685, 210,40, 150);
            Image grassImg1 = Image.FromFile("Img/grass1.png");
            g.DrawImage(grassImg1, grass4.Rectangle);
            Grass grass5 = new Grass(0, 455, 40, 150);
            g.DrawImage(grassImg1, grass5.Rectangle);
            Grass grass6 = new Grass(0, 115, 40, 150);
            g.DrawImage(grassImg1, grass6.Rectangle);
            Grass grass7 = new Grass(160, 320, 120, 30);
            g.DrawImage(grassImg, grass7.Rectangle);
            Grass grass8 = new Grass(540, 320, 120, 30);
            g.DrawImage(grassImg, grass8.Rectangle);
            Grass grass9 = new Grass(333, 60, 150, 50);
            g.DrawImage(grassImg, grass9.Rectangle);

            Image stellImg = Image.FromFile("Img/stell1.png");
            Stell stell = new Stell(0, 280);
            g.DrawImage(stellImg, stell.Rectangle);
            controller.stellList.Add(stell);
            Stell stell2 = new Stell(80, 550);
            g.DrawImage(stellImg, stell2.Rectangle);
            controller.stellList.Add(stell2);
            Stell stell3 = new Stell(755, 295);
            g.DrawImage(stellImg, stell3.Rectangle);
            controller.stellList.Add(stell3);
        }

        //按键抬起事件
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                    controller.upPress = false;
                    break;
                case Keys.Down:
                    controller.downPress = false;
                    break;
                case Keys.Left:
                    controller.leftPress = false;
                    break;
                case Keys.Right:
                    controller.rightPress = false;
                    break;
                default:
                    break;
            }
        }

        //绘制地图
        private void drawMap(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            //统一绘制
            Image wallImg = Image.FromFile("Img/wall.png");
            for (int i = 0; i < controller.wallList.Count; i++)
            {
                try
                {
                    g.DrawImage(wallImg, controller.wallList[i].Rectangle);
                }catch(Exception xe) { }
            }

            //boss
            Image bossImg = Image.FromFile("Img/boss.png");
            g.DrawImage(bossImg, controller.boss.Rectangle);
            fresh = false;
        }

        //添加墙
        private void addWall()
        {
            Wall wall1 = new Wall(360, 585); controller.wallList.Add(wall1);
            Wall wall2 = new Wall(360, 565); controller.wallList.Add(wall2);
            Wall wall3 = new Wall(360, 545); controller.wallList.Add(wall3);
            Wall wall4 = new Wall(360, 525); controller.wallList.Add(wall4);
            Wall wall5 = new Wall(380, 525); controller.wallList.Add(wall5);
            Wall wall6 = new Wall(400, 525); controller.wallList.Add(wall6);
            Wall wall7 = new Wall(420, 525); controller.wallList.Add(wall7);
            Wall wall8 = new Wall(440, 525); controller.wallList.Add(wall8);
            Wall wall9 = new Wall(440, 545); controller.wallList.Add(wall9);
            Wall wall10 = new Wall(440, 565); controller.wallList.Add(wall10);
            Wall wall11 = new Wall(440, 585); controller.wallList.Add(wall11);
            //中间H型墙
            for (int k = 0; k < 15; k++)
            {
                Wall Hwalll1 = new Wall(350, 150 + k * 20); controller.wallList.Add(Hwalll1);
                Wall Hwalll2 = new Wall(370, 150 + k * 20); controller.wallList.Add(Hwalll2);
                Wall Hwallr1 = new Wall(430, 150 + k * 20); controller.wallList.Add(Hwallr1);
                Wall Hwallr2 = new Wall(450, 150 + k * 20); controller.wallList.Add(Hwallr2);
            }
            Wall Cwall0 = new Wall(350, 300); controller.wallList.Add(Cwall0);
            Wall Cwall1 = new Wall(370, 300); controller.wallList.Add(Cwall1);
            Wall Cwall2 = new Wall(390, 300); controller.wallList.Add(Cwall2);
            Wall Cwall3 = new Wall(410, 300); controller.wallList.Add(Cwall3);
            Wall Cwall4 = new Wall(430, 300); controller.wallList.Add(Cwall4);
            Wall Cwall5 = new Wall(450, 300); controller.wallList.Add(Cwall5);
            for (int n = 0; n < wallMapList.Count; n++)
            {
                Wall wall = new Wall(wallMapList[n].X, wallMapList[n].Y);
                controller.wallList.Add(wall);
            }
        }

        //窗体关闭事件
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            controller.gameRun = false;
            for(int i = 0; i < controller.enemyTank.Count; i++)
            {
                controller.enemyTank[i].IsOver = false;
            }
        }
    }
}
