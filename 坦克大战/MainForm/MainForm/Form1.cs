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

namespace MainForm
{
    public partial class Form1 : Form
    {
       static Point tankPoint = new Point(320, 560);//坦克位置
        Boss boss = new Boss(390, 560);//大王位置
        RotateFlipType rotate = RotateFlipType.RotateNoneFlipX;//控制坦克旋转
        Tank playerTank = new Tank(tankPoint.X, tankPoint.Y, 0, 0);//创建玩家坦克
        List<Tank> enemyTank = new List<Tank>();//创建敌方坦克
        List<Panel> enemyPanel = new List<Panel>();//创建敌方坦克容器
        List<Bullet> enemyBulletList = new List<Bullet>();//创建敌方子弹集合
        List<Panel> enemyBulletPanelList =new List<Panel>();//创建敌方子弹容器集合
        List<Bullet> playerBulletList = new List<Bullet>();//创建本方子弹集合
        List<Panel> playerBulletPanelList = new List<Panel>();//创建本方子弹容器集合
        List<Wall> wallList = new List<Wall>();//墙的集合
        List<Stell> stellList = new List<Stell>();//钢集合
        List<Grass> grassList = new List<Grass>();//草集合
        bool playerBulletIsOver = true;//判断本方子弹线程是否结束,防止玩家过于变态,增加游戏难度!
        //控制本方坦克的移动
        bool upPress = false;
        bool downPress = false;
        bool leftPress = false;
        bool rightPress = false; 
        bool isPlayerBullet = false;
        int playerHited = 0;
        bool gameRun = true;//游戏运行状态
        int hitCount = 0;//击中坦克数量
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
        }

        
        private void Form1_Load(object sender, EventArgs e)
        {
            this.KeyPreview = true;
            //玩家控制坦克移动
            Thread playerRunThread = new Thread(new ThreadStart(playerRun));
            playerRunThread.Start();

            //控制敌方坦克移动
            Thread enemyRunThread = new Thread(new ThreadStart(enemyRuns));
            enemyRunThread.Start();
        }

        //利用线程控制玩家移动
        private void playerRun()
        {
            while (gameRun)
            {
                playerMove();
                Thread.Sleep(10);
            }
        }

        //利用线程控制子弹移动
        private void bulletRun(Object tank)
        {
            bool isOver = true;
            while (isOver)
            {
                bulletMove(tank);
                Thread.Sleep(100);
            }
        }

        //玩家坦克移动
        private void playerMove()
        {
            //玩家被击中，重生
            if (playerTankHitIntersect(playerTank, enemyTank, enemyBulletList))
            {
                if (playerHited == 2)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        enemyTank[i].IsOver = false;
                    }
                    gameRun = false;
                    MessageBox.Show("Game Over!");
                }
                playerHited++;
                this.Invalidate(playerTank.Rectangle);
                playerTank.X = 320; playerTank.Y = 560;
                playerTank.Direction = 0;
                playerTank.refreshRectangle(playerTank.X, playerTank.Y);
                this.Invalidate(playerTank.Rectangle);
            }
            DoubleBuffer(playertankPanel);
            if (hitCount == 4)
            {
                Thread.Sleep(10000);
            }
            if (upPress)
            {
                playerTank.Direction = 0;
                if (playerTank.Y >= 0)
                {
                    Rectangle rec = new Rectangle(playerTank.X, playerTank.Y - 2, playerTank.Width, playerTank.Height);
                    if (wallTankHit(rec, wallList) || stellTankHit(rec, stellList))
                    { }
                    else
                    {
                        playerTank.Y -= 2;
                    }
                    rotate = RotateFlipType.RotateNoneFlipX;
                    playerTank.refreshRectangle(playerTank.X, playerTank.Y);
                    this.Invalidate(playerTank.Rectangle);
                }
            }
            else if (downPress)
            {
                playerTank.Direction = 1;
                if (playerTank.Y <= 570)
                {
                    Rectangle rec = new Rectangle(playerTank.X, playerTank.Y + 2, playerTank.Width, playerTank.Height);
                    if (wallTankHit(rec, wallList) || stellTankHit(rec, stellList))
                    {      }
                    else
                    {
                        playerTank.Y += 2;
                    }
                    rotate = RotateFlipType.Rotate180FlipX;
                    playerTank.refreshRectangle(playerTank.X, playerTank.Y);
                    this.Invalidate(playerTank.Rectangle);
                }
            }
            else if (leftPress)
            {
                playerTank.Direction = 2;
                if (playerTank.X >= 0)
                {
                    Rectangle rec = new Rectangle(playerTank.X-2, playerTank.Y, playerTank.Width, playerTank.Height);
                    if (wallTankHit(rec, wallList) || stellTankHit(rec, stellList))
                    { }
                    else
                    {
                        playerTank.X -= 2;
                    }
                    rotate = RotateFlipType.Rotate90FlipX;
                    playerTank.refreshRectangle(playerTank.X, playerTank.Y);
                    this.Invalidate(playerTank.Rectangle);
                }
            }
            else if(rightPress)
            {
                playerTank.Direction = 3;
                if (playerTank.X <= 800)
                {
                    Rectangle rec = new Rectangle(playerTank.X + 2, playerTank.Y, playerTank.Width, playerTank.Height);
                    if (wallTankHit(rec, wallList) || stellTankHit(rec, stellList))
                    {}
                    else
                    {
                        playerTank.X += 2;
                    }
                    rotate = RotateFlipType.Rotate90FlipY;
                    playerTank.refreshRectangle(playerTank.X, playerTank.Y);
                    this.Invalidate(playerTank.Rectangle);
                }
            }
        }

        //敌方坦克及子弹在线程中运动
        private void enemyRuns()
        {
            bool drawOver = false;//绘图是否结束
            while (!drawOver)
            {
                if (enemyTank.Count>0)
                {
                    for (int i = 0; i < enemyTank.Count; i++)
                    {
                        Thread enemyRunThread = new Thread(new ParameterizedThreadStart(enemyMove));
                        int o = i;
                        enemyRunThread.Start((object)o);
                        //敌方子弹移动
                        Thread bulletThread = new Thread(new ParameterizedThreadStart(bulletMove));
                        bulletThread.Start((object)enemyTank[i]);
                        Thread.Sleep(20);
                    }
                    if (enemyTank.Count == 4)
                    {
                        drawOver = true;
                    }
                }
            }
        }

        //敌方坦克移动
        private void enemyMove(object j)
        {
            int i = (int)j;
            bool isRunning = true;
            while (isRunning&&enemyTank[i].IsOver)
            {
                Random random = new Random();
                int direction = random.Next(0, 4);
                enemyTank[i].Direction = direction;
                if (enemyTank[i].Direction == 0)
                {
                    int y1 = random.Next(0, enemyTank[i].Y);
                    while (isRunning && enemyTank[i].Y > y1 && enemyTank[i].Y > 4)
                    {
                        Rectangle rec = new Rectangle(enemyTank[i].X, enemyTank[i].Y - 1, enemyTank[i].Width, enemyTank[i].Height);
                        if (enemyTankHitIntersect(rec, enemyTank, i)||wallTankHit(rec,wallList)||stellTankHit(rec,stellList))
                        {
                            break;
                        }
                        enemyTank[i].Y -= 1;
                        enemyTank[i].refreshRectangle(enemyTank[i].X, enemyTank[i].Y);
                        Invalidate(enemyTank[i].Rectangle);
                        if (playerBulletHitEnemy(playerBulletList, enemyTank[i]) != -1)
                        {
                            int hitNum = playerBulletHitEnemy(playerBulletList, enemyTank[i]);
                            isRunning = false;
                            enemyTank[i].IsOver = false;
                            enemyTank[i].refreshRectangle(-100, -100);
                            this.Invalidate(enemyTank[i].Rectangle);
                            this.Invalidate(playerBulletList[hitNum].Rectangle);
                            playerBulletList.RemoveAt(hitNum);
                            hitCount++;
                            if (hitCount == 4)
                            {
                                MessageBox.Show("You Win!");
                            }
                        }
                        Thread.Sleep(10);
                    }
                }
                else if (enemyTank[i].Direction == 1)
                {
                    int y2 = random.Next(enemyTank[i].Y, 580);
                    while (isRunning && enemyTank[i].Y < y2 && enemyTank[i].Y < 580)
                    {
                        Rectangle rec = new Rectangle(enemyTank[i].X, enemyTank[i].Y + 1, enemyTank[i].Width, enemyTank[i].Height);
                        if (enemyTankHitIntersect(rec, enemyTank, i) || wallTankHit(rec, wallList)  || stellTankHit(rec, stellList))
                        {
                            break;
                        }
                        enemyTank[i].Y += 1;
                        enemyTank[i].refreshRectangle(enemyTank[i].X, enemyTank[i].Y);
                        Invalidate(enemyTank[i].Rectangle);
                        if (playerBulletHitEnemy(playerBulletList, enemyTank[i]) != -1)
                        {
                            int hitNum = playerBulletHitEnemy(playerBulletList, enemyTank[i]);
                            isRunning = false;
                            enemyTank[i].IsOver = false;
                            enemyTank[i].refreshRectangle(-100, -100);
                            this.Invalidate(enemyTank[i].Rectangle);
                            this.Invalidate(playerBulletList[hitNum].Rectangle);
                            playerBulletList.RemoveAt(hitNum);
                            hitCount++;
                            if (hitCount == 4)
                            {
                                MessageBox.Show("You Win!");
                            }
                        }
                        Thread.Sleep(10);
                    }
                }
                else if (enemyTank[i].Direction == 2)
                {
                    int x1 = random.Next(0, enemyTank[i].X + 1);
                    while (isRunning && enemyTank[i].X > x1 && enemyTank[i].X > 4)
                    {
                        Rectangle rec = new Rectangle(enemyTank[i].X-1, enemyTank[i].Y, enemyTank[i].Width, enemyTank[i].Height);
                        if (enemyTankHitIntersect(rec, enemyTank, i) || wallTankHit(rec, wallList)|| stellTankHit(rec, stellList))
                        {
                            break;
                        }
                        enemyTank[i].X -= 1;
                        enemyTank[i].refreshRectangle(enemyTank[i].X, enemyTank[i].Y);
                        Invalidate(enemyTank[i].Rectangle);
                        if (playerBulletHitEnemy(playerBulletList, enemyTank[i]) != -1)
                        {
                            int hitNum = playerBulletHitEnemy(playerBulletList, enemyTank[i]);
                            isRunning = false;
                            enemyTank[i].IsOver = false;
                            enemyTank[i].refreshRectangle(-100, -100);
                            this.Invalidate(enemyTank[i].Rectangle);
                            this.Invalidate(playerBulletList[hitNum].Rectangle);
                            playerBulletList.RemoveAt(hitNum);
                            hitCount++;
                            if (hitCount == 4)
                            {
                                MessageBox.Show("You Win!");
                            }
                        }
                        Thread.Sleep(10);
                    }
                }
                else
                {
                    int x2 = random.Next(enemyTank[i].X, 800);
                    while (isRunning && enemyTank[i].X < x2 && enemyTank[i].X < 800)
                    {
                        Rectangle rec = new Rectangle(enemyTank[i].X+1, enemyTank[i].Y, enemyTank[i].Width, enemyTank[i].Height);
                        if (enemyTankHitIntersect(rec, enemyTank, i) || wallTankHit(rec, wallList) || stellTankHit(rec, stellList))
                        {
                            break;
                        }
                        enemyTank[i].X += 1;
                        //引用传值貌似会卡暂时不封装
                        enemyTank[i].refreshRectangle(enemyTank[i].X, enemyTank[i].Y);
                        Invalidate(enemyTank[i].Rectangle);
                        if (playerBulletHitEnemy(playerBulletList, enemyTank[i]) != -1)
                        {
                            int hitNum = playerBulletHitEnemy(playerBulletList, enemyTank[i]);
                            isRunning = false;
                            enemyTank[i].IsOver = false;
                            enemyTank[i].refreshRectangle(-100, -100);
                            this.Invalidate(enemyTank[i].Rectangle);
                            this.Invalidate(playerBulletList[hitNum].Rectangle);
                            playerBulletList.RemoveAt(hitNum);
                            hitCount++;
                            if (!gameRun)
                            {
                                isRunning = false;
                            }
                            if (hitCount == 4)
                            {
                                MessageBox.Show("You Win!");
                            }
                        }
                        Thread.Sleep(10);
                    }
                }
                //Thread.Sleep(10); 
            }
        }

        //键盘事件
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                    playerTank.IsLaunch = false;
                    upPress = true;
                    break;
                case Keys.Down:
                    playerTank.IsLaunch = false;
                    downPress = true;
                    break;
                case Keys.Left:
                    playerTank.IsLaunch = false;
                    leftPress = true;
                    break;
                case Keys.Right:
                    playerTank.IsLaunch = false;
                    rightPress = true;
                    break;
                case Keys.Space:
                    if (hitCount == 4)
                    {
                        Thread.Sleep(10000);
                    }
                    if (playerBulletIsOver)
                    {
                        Thread bulletThread = new Thread(new ParameterizedThreadStart(playerBulletRun));//创建一个线程
                        bulletThread.Start((object)playerTank);                  //开启线程
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
            tankeImg.RotateFlip(rotate);
            g.DrawImage(tankeImg, playerTank.Rectangle);
            Image playerBulletImage = Image.FromFile("Img/bullet1.png");
            for (int k = 0; k < playerBulletList.Count; k++)
            {
                if (k < playerBulletList.Count)
                {
                    try
                    {
                        playerBulletImage.RotateFlip(rotateMap[playerBulletList[k].Direction]);
                        g.DrawImage(playerBulletImage, playerBulletList[k].Rectangle);
                    }
                    catch (Exception ex)
                    {                    }
                }
            }
            if (fresh)
            {
                drawMap(e);
            }
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
                } while (makeTankHit(rec, makeTank) || wallTankHit(rec, wallList));
                if (enemyTank.Count < 4)
                {
                    makeTank.Add(rec);
                }
                int dirc = random.Next(0, 4);
                Tank tank = new Tank(x, y, 1, dirc);
                tank.Num = i;
                if (enemyTank.Count < 4)
                {
                    enemyTank.Add(tank);
                }
                Image tankImg = Image.FromFile("Img/enemytank.png");
                tankImg.RotateFlip(rotateMap[enemyTank[i].Direction]);
                g.DrawImage(tankImg, enemyTank[i].Rectangle);
            }
            for (int j = 0; j < enemyBulletList.Count; j++)
            {
                if (j < enemyBulletList.Count)
                {
                    try   //由于线程执行顺序的不确定性，移除子弹的时候长度减小，会越界，较难解决，先用异常处理
                    {
                        Image bulletImage = Image.FromFile("Img/bullet1.png");
                        bulletImage.RotateFlip(rotateMap[enemyBulletList[j].Direction]);
                        g.DrawImage(bulletImage, enemyBulletList[j].Rectangle);
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
            stellList.Add(stell);
            Stell stell2 = new Stell(80, 550);
            g.DrawImage(stellImg, stell2.Rectangle);
            stellList.Add(stell2);
            Stell stell3 = new Stell(755, 295);
            g.DrawImage(stellImg, stell3.Rectangle);
            stellList.Add(stell3);
        }

        //玩家子弹移动
        private void playerBulletRun(object tank)
        {
            playerBulletIsOver = false;
            isPlayerBullet = true;
            bulletMove(tank);
            playerBulletIsOver = true;
            isPlayerBullet = false;
        }

        //子弹移动
        private void bulletMove(object tanks)
        {
            Tank tank = (Tank)tanks;
            while (tank.IsOver)
            {
                Bullet bullet = new Bullet(tank.X, tank.Y);
                switch (tank.Direction)
                {
                    case 0:
                        bullet.Y -= 10;
                        bullet.X += 10;
                        break;
                    case 1:
                        bullet.Y += 10;
                        bullet.X += 10;
                        break;
                    case 2:
                        bullet.X -= 10;
                        bullet.Y += 10;
                        Rectangle rec = new Rectangle(bullet.X, bullet.Y, 20, 12);
                        bullet.Rectangle = rec;
                        break;
                    case 3:
                        bullet.X +=10;
                        bullet.Y += 10;
                        Rectangle recs = new Rectangle(bullet.X, bullet.Y, 20, 12);
                        bullet.Rectangle = recs;
                        break;
                    default:
                        break;
                }
                bullet.Direction = tank.Direction;
                bullet.refreshRectangle(bullet.X,bullet.Y);
                if (tank.Type == 1)
                {
                    enemyBulletList.Add(bullet);
                }
                else
                {
                    playerBulletList.Add(bullet);
                }
                switch (tank.Direction)
                {
                    case 0:
                        while (!bullet.IsOver && bullet.Y > 0)
                        {
                            if (enemyBulletStellHit(bullet.Rectangle,stellList))
                            {
                                break;
                            }
                            bullet.Y -= 4;
                            bullet.refreshRectangle(bullet.X, bullet.Y);
                            Rectangle rec = new Rectangle(bullet.X, bullet.Y, 15, 25);
                            Invalidate(rec);
                            bulletRefreshAndHit(bullet, tank);
                            Thread.Sleep(10);  //线程休眠20毫秒
                        }
                        break;
                    case 1:
                        while (!bullet.IsOver && bullet.Y < 580)
                        {
                            if (enemyBulletStellHit(bullet.Rectangle, stellList))
                            {
                                break;
                            }
                            bullet.Y += 4;
                            bullet.refreshRectangle(bullet.X, bullet.Y);
                            Rectangle rec = new Rectangle(bullet.X, bullet.Y - 4, 15, 25);
                            Invalidate(rec);
                            bulletRefreshAndHit(bullet, tank);
                            Thread.Sleep(10);  //线程休眠20毫秒
                        }
                        break;
                    case 2:
                        while (!bullet.IsOver && bullet.X > 0)
                        {
                            if (enemyBulletStellHit(bullet.Rectangle, stellList))
                            {
                                break;
                            }
                            bullet.X -= 4;
                            bullet.refreshRectangle(bullet.X, bullet.Y);
                            Rectangle rec = new Rectangle(bullet.X, bullet.Y, 25, 10);
                            Invalidate(rec);
                            bulletRefreshAndHit(bullet, tank);
                            Thread.Sleep(10);  //线程休眠20毫秒
                        }
                        break;
                    case 3:
                        while (!bullet.IsOver&&bullet.X < 800)
                        {
                            if ( enemyBulletStellHit(bullet.Rectangle, stellList))
                            {
                                break;
                            }
                            bullet.X += 4;
                            bullet.refreshRectangle(bullet.X, bullet.Y);
                            Rectangle rec = new Rectangle(bullet.X - 4, bullet.Y, 25, 10);
                            Invalidate(rec);
                            bulletRefreshAndHit(bullet, tank);
                            Thread.Sleep(10);  //线程休眠20毫秒
                        }
                        break;
                    default:
                        break;
                }
                this.Invalidate(bullet.Rectangle);
                if (tank.Type == 1)
                {
                    enemyBulletList.Remove(bullet);
                }
                else
                {
                    playerBulletList.Remove(bullet);
                }
                if (tank.Type == 0)
                {
                    break;
                }
                Thread.Sleep(200);
            }
        }

        //封装子弹刷新及碰撞方法
        private void bulletRefreshAndHit(Bullet bullet,Tank tank)
        {
            if (bulletBossHit(bullet, boss))
            {
                bullet.IsOver = true;
                for(int i = 0; i < 4; i++)
                {
                    enemyTank[i].IsOver = false;
                }
                gameRun = false;
                MessageBox.Show("Game Over!");
            }
            if (enemyBulletWallHit(bullet, wallList) != -1)
            {
                int i = enemyBulletWallHit(bullet, wallList);
                this.Invalidate(wallList[i].Rectangle);
                wallList.RemoveAt(i);
                bullet.IsOver = true;
            }
            if (tank.Type == 0)
            {
                int i = bulletHit(bullet, enemyBulletList);
                if (i != -1)
                {
                    this.Invalidate(enemyBulletList[i].Rectangle);
                    enemyBulletList.RemoveAt(i);
                    bullet.IsOver = true;
                }
            }
            else
            {
                int i = bulletHit(bullet, playerBulletList);
                if (i != -1)
                {
                    this.Invalidate(playerBulletList[i].Rectangle);
                    playerBulletList.RemoveAt(i);
                    bullet.IsOver = true;
                }
            }
        }

        //双缓冲方法，解决闪烁问题
        private void DoubleBuffer(Control control)
        {
            control.GetType().GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance
| System.Reflection.BindingFlags.NonPublic).SetValue(control, true, null);
        }

        //玩家坦克碰撞检测
        private bool playerTankHitIntersect(Tank tank,List<Tank> enemyList,List<Bullet> bulletList)
        {
            for(int i = 0; i < enemyList.Count; i++)
            {
                if (tank.Rectangle.IntersectsWith(enemyList[i].Rectangle))
                {
                    return true;
                }
            }
            for(int i = 0; i < bulletList.Count; i++)
            {
                try
                {
                    if (tank.Rectangle.IntersectsWith(bulletList[i].Rectangle))
                    {
                        return true;
                    }
                }catch(Exception ex)
                {

                }
            }
            return false;
        }

        //敌方坦克之间碰撞检测
        private bool enemyTankHitIntersect(Rectangle rectangle,List<Tank> tankList,int k)
        {
            for(int i = 0; i < tankList.Count; i++)
            {
                try
                {
                    if (i != k && rectangle.IntersectsWith(tankList[i].Rectangle))
                    {
                        return true;
                    }
                }catch(Exception ex) { }
            }
            return false;
        }

        //敌方坦克与我方子弹碰撞检测
         private int playerBulletHitEnemy(List<Bullet> bulletList,Tank tank)
        {
            for (int j = 0; j < bulletList.Count; j++)
            {
                try
                {
                    if (bulletList[j].Rectangle.IntersectsWith(tank.Rectangle))
                    {
                        return j;
                    }
                }catch(Exception ex) { }
            }
            return -1;
        }

        //子弹间碰撞检测
        private int bulletHit(Bullet bullet,List<Bullet> list)
        {
            for(int i = 0; i < list.Count; i++)
            {
                try
                {
                    if (bullet.Rectangle.IntersectsWith(list[i].Rectangle))
                    {
                        return i;
                    }
                }catch(Exception ex) { }
            }
            return -1;
        }

        //坦克与墙的碰撞
        private bool wallTankHit(Rectangle rectangle,List<Wall> wallList)
        {
            for(int i = 0; i < wallList.Count; i++)
            {
                try
                {
                    if (rectangle.IntersectsWith(wallList[i].Rectangle))
                    {
                        return true;
                    }
                }catch(Exception ex) { }
            }
            return false;
        }

        //坦克与钢的碰撞
        private bool stellTankHit(Rectangle rectangle,List<Stell> stellList)
        {
            for(int i = 0; i < stellList.Count; i++)
            {
                try
                {
                    if (rectangle.IntersectsWith(stellList[i].Rectangle))
                    {
                        return true;
                    }
                }catch(Exception ex) { }
            }
            return false;
        }

        //生成坦克位置碰撞检测
        private bool makeTankHit(Rectangle rec,List<Rectangle> list)
        {
            for(int i = 0; i < list.Count; i++)
            {
                try
                {
                    if (rec.IntersectsWith(list[i]))
                    {
                        return true;
                    }
                }catch(Exception ex) { }
            }
            return false;
        }

        //敌方子弹与墙的碰撞检测,与本方分开，减少一些循环
        private int enemyBulletWallHit(Bullet bullet,List<Wall> wallList)
        {
            for(int i = 0; i < wallList.Count; i++)
            {
                try
                {
                    if (bullet.Rectangle.IntersectsWith(wallList[i].Rectangle))
                    {
                        return i;
                    }
                }catch(Exception ex) { }
            }
            return -1;
        }

        //敌方子弹与钢的碰撞检测
        private bool enemyBulletStellHit(Rectangle rec,List<Stell> stellList)
        {
            for(int i = 0; i < stellList.Count; i++)
            {
                try
                {
                    if (rec.IntersectsWith(stellList[i].Rectangle))
                    {
                        return true;
                    }
                }catch(Exception ex) { }
            }
            return false;
        }

        //子弹与大王的碰撞检测
        private bool bulletBossHit(Bullet bullet,Boss boss) 
        {
            if (bullet.Rectangle.IntersectsWith(boss.Rectangle))
            {
                return true;
            }
            return false;
        }

        //按键抬起事件
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                    upPress = false;
                    break;
                case Keys.Down:
                    downPress = false;
                    break;
                case Keys.Left:
                    leftPress = false;
                    break;
                case Keys.Right:
                    rightPress = false;
                    break;
                default:
                    break;
            }
        }

        //绘制地图
        private void drawMap(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Wall wall1 = new Wall(360, 585); wallList.Add(wall1);
            Wall wall2 = new Wall(360, 565); wallList.Add(wall2);
            Wall wall3 = new Wall(360, 545); wallList.Add(wall3);
            Wall wall4 = new Wall(360, 525); wallList.Add(wall4);
            Wall wall5 = new Wall(380, 525); wallList.Add(wall5);
            Wall wall6 = new Wall(400, 525); wallList.Add(wall6);
            Wall wall7 = new Wall(420, 525); wallList.Add(wall7);
            Wall wall8 = new Wall(440, 525); wallList.Add(wall8);
            Wall wall9 = new Wall(440, 545); wallList.Add(wall9);
            Wall wall10 = new Wall(440, 565); wallList.Add(wall10);
            Wall wall11 = new Wall(440, 585); wallList.Add(wall11);
            //中间H型墙
            for(int k = 0; k < 15; k++)
            {
                Wall Hwalll1 = new Wall(350, 150 + k * 20); wallList.Add(Hwalll1);
                Wall Hwalll2 = new Wall(370, 150 + k * 20); wallList.Add(Hwalll2);
                Wall Hwallr1 = new Wall(430, 150 + k * 20); wallList.Add(Hwallr1);
                Wall Hwallr2 = new Wall(450, 150 + k * 20); wallList.Add(Hwallr2);
            }
            Wall Cwall0 = new Wall(350, 300); wallList.Add(Cwall0);
            Wall Cwall1 = new Wall(370, 300); wallList.Add(Cwall1);
            Wall Cwall2 = new Wall(390, 300); wallList.Add(Cwall2);
            Wall Cwall3 = new Wall(410, 300); wallList.Add(Cwall3);
            Wall Cwall4 = new Wall(430, 300); wallList.Add(Cwall4);
            Wall Cwall5 = new Wall(450, 300); wallList.Add(Cwall5);

            //根据墙map绘制
            for (int n = 0; n < wallMapList.Count; n++)
            {
                Wall wall = new Wall(wallMapList[n].X, wallMapList[n].Y);
                wallList.Add(wall);
            }


            //统一绘制
            Image wallImg = Image.FromFile("Img/wall.png");
            for (int i = 0; i < wallList.Count; i++)
            {
                g.DrawImage(wallImg, wallList[i].Rectangle);
            }

            //boss
            Image bossImg = Image.FromFile("Img/boss.png");
            g.DrawImage(bossImg, boss.Rectangle);
            fresh = false;
        }

        //窗体关闭事件
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            gameRun = false;
        }
    }
}
