using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MainForm
{
    class Controller
    {
        public Boss boss = new Boss(390, 560);//大王位置
        public List<Wall> wallList = new List<Wall>();//墙的集合
        public List<Stell> stellList = new List<Stell>();//钢集合
        public List<Grass> grassList = new List<Grass>();//草集合
        public RotateFlipType rotate = RotateFlipType.RotateNoneFlipX;//控制坦克旋转
        static Point tankPoint = new Point(320, 560);//坦克位置
        public Tank playerTank = new Tank(tankPoint.X, tankPoint.Y, 0, 0);//创建玩家坦克
        public List<Tank> enemyTank = new List<Tank>();//创建敌方坦克
        public List<Bullet> enemyBulletList = new List<Bullet>();//创建敌方子弹集合
        public List<Bullet> playerBulletList = new List<Bullet>();//创建本方子弹集合
        public bool playerBulletIsOver = true;//判断本方子弹线程是否结束,防止玩家过于变态,增加游戏难度!
        //全局布尔变量，解决键盘监听阻塞问题
        public bool upPress = false;
        public bool downPress = false;
        public bool leftPress = false;
        public bool rightPress = false;
        public bool isPlayerBullet = false;
        public int playerHited = 0;
        public bool gameRun = true;//游戏运行状态
        public int hitCount = 0;//击中坦克数量

        //利用线程控制玩家移动
        public void playerRun()
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
                playerTank.X = 320; playerTank.Y = 560;
                playerTank.Direction = 0;
                playerTank.refreshRectangle(playerTank.X, playerTank.Y);
                
            }
            if (hitCount == 4)
            {
                Thread.Sleep(10000);
            }
            if (upPress)
            {
                
                if (playerTank.Y >= 0)
                {
                    myTankMove(upPress, downPress, leftPress, rightPress, playerTank, wallList, stellList);
                    rotate = RotateFlipType.RotateNoneFlipX;
                }
            }
            else if (downPress)
            {
                if (playerTank.Y <= 570)
                {
                    myTankMove(upPress, downPress, leftPress, rightPress, playerTank, wallList, stellList);
                    rotate = RotateFlipType.Rotate180FlipX;             
                }
            }
            else if (leftPress)
            {
                if (playerTank.X >= 0)
                {
                    myTankMove(upPress, downPress, leftPress, rightPress, playerTank, wallList, stellList);
                    rotate = RotateFlipType.Rotate90FlipX; 
                }
            }
            else if (rightPress)
            {
                if (playerTank.X <= 800)
                {
                    myTankMove(upPress, downPress, leftPress, rightPress, playerTank, wallList, stellList);
                    rotate = RotateFlipType.Rotate90FlipY;   
                }
            }
        }

        //敌方坦克及子弹在线程中运动
        public void enemyRuns()
        {
            bool drawOver = false;//绘图是否结束
            while (!drawOver)
            {
                if (enemyTank.Count > 0)
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
            while (enemyTank[i].IsOver)
            {
                Random random = new Random();
                int direction = random.Next(0, 4);
                enemyTank[i].Direction = direction;
                if (enemyTank[i].Direction == 0)
                {
                    int y1 = random.Next(0, enemyTank[i].Y);
                    while (enemyTank[i].Y > y1 && enemyTank[i].Y > 4)
                    {
                        Rectangle rec = new Rectangle(enemyTank[i].X, enemyTank[i].Y - 1, enemyTank[i].Width, enemyTank[i].Height);
                        if (enemyTankHitIntersect(rec, enemyTank, i) || wallTankHit(rec, wallList) || stellTankHit(rec, stellList))
                        {
                            break;
                        }
                        enemyTank[i].Y -= 1;
                        enemyDisapper(enemyTank[i]);//提炼函数，去除重复代码
                        if (!enemyTank[i].IsOver)
                        {
                            break;
                        }
                    }
                }
                else if (enemyTank[i].Direction == 1)
                {
                    int y2 = random.Next(enemyTank[i].Y, 580);
                    while (enemyTank[i].Y < y2 && enemyTank[i].Y < 580)
                    {
                        Rectangle rec = new Rectangle(enemyTank[i].X, enemyTank[i].Y + 1, enemyTank[i].Width, enemyTank[i].Height);
                        if (enemyTankHitIntersect(rec, enemyTank, i) || wallTankHit(rec, wallList) || stellTankHit(rec, stellList))
                        {
                            break;
                        }
                        enemyTank[i].Y += 1;
                        enemyDisapper(enemyTank[i]);
                        if (!enemyTank[i].IsOver)
                        {
                            break;
                        }
                    }
                }
                else if (enemyTank[i].Direction == 2)
                {
                    int x1 = random.Next(0, enemyTank[i].X + 1);
                    while (enemyTank[i].X > x1 && enemyTank[i].X > 4)
                    {
                        Rectangle rec = new Rectangle(enemyTank[i].X - 1, enemyTank[i].Y, enemyTank[i].Width, enemyTank[i].Height);
                        if (enemyTankHitIntersect(rec, enemyTank, i) || wallTankHit(rec, wallList) || stellTankHit(rec, stellList))
                        {
                            break;
                        }
                        enemyTank[i].X -= 1;
                        enemyDisapper(enemyTank[i]);
                        if (!enemyTank[i].IsOver)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    int x2 = random.Next(enemyTank[i].X, 800);
                    while (enemyTank[i].X < x2 && enemyTank[i].X < 800)
                    {
                        Rectangle rec = new Rectangle(enemyTank[i].X + 1, enemyTank[i].Y, enemyTank[i].Width, enemyTank[i].Height);
                        if (enemyTankHitIntersect(rec, enemyTank, i) || wallTankHit(rec, wallList) || stellTankHit(rec, stellList))
                        {
                            break;
                        }
                        enemyTank[i].X += 1;
                        enemyDisapper(enemyTank[i]);
                        if (!enemyTank[i].IsOver)
                        {
                            break;
                        }
                    }
                }
            }
            //Thread.Sleep(10);
        }

        //提炼敌方坦克刷新及消失判断方法
        private void enemyDisapper(Tank tank)
        {
            tank.refreshRectangle(tank.X, tank.Y);   
            if (playerBulletHitEnemy(playerBulletList, tank) !=null)
            {
                Bullet hitNum = playerBulletHitEnemy(playerBulletList, tank);
                tank.IsOver = false;
                tank.refreshRectangle(-100, -100);
                playerBulletList.Remove(hitNum);
                hitCount++;
                if (hitCount == 4)
                {
                    MessageBox.Show("You Win!");
                }
            }
            Thread.Sleep(10);
        }

        ////玩家子弹移动
        public void playerBulletRun(object tank)
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
                        bullet.X += 10;
                        bullet.Y += 10;
                        Rectangle recs = new Rectangle(bullet.X, bullet.Y, 20, 12);
                        bullet.Rectangle = recs;
                        break;
                    default:
                        break;
                }
                bullet.Direction = tank.Direction;
                bullet.refreshRectangle(bullet.X, bullet.Y);
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
                            if (enemyBulletStellHit(bullet.Rectangle, stellList))
                            {
                                break;
                            }
                            bullet.Y -= 4;
                            //不好提炼，子弹根据方向绘制，每个方向矩形区域不同
                            bullet.refreshRectangle(bullet.X, bullet.Y); 
                            bulletRefreshAndHit(bullet, tank);
                            Thread.Sleep(10);  //线程休眠10毫秒
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
                            bulletRefreshAndHit(bullet, tank);
                            Thread.Sleep(10);  //线程休眠10毫秒
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
                            bulletRefreshAndHit(bullet, tank);
                            Thread.Sleep(10);  //线程休眠10毫秒
                        }
                        break;
                    case 3:
                        while (!bullet.IsOver && bullet.X < 800)
                        {
                            if (enemyBulletStellHit(bullet.Rectangle, stellList))
                            {
                                break;
                            }
                            bullet.X += 4;
                            bullet.refreshRectangle(bullet.X, bullet.Y);
                            bulletRefreshAndHit(bullet, tank);
                            Thread.Sleep(10);  //线程休眠10毫秒
                        }
                        break;
                    default:
                        break;
                }
                if (tank.Type == 1)
                {
                    try
                    {
                        enemyBulletList.Remove(bullet);
                    }catch(Exception e) { }
                }
                else
                {
                    try
                    {
                        playerBulletList.Remove(bullet);
                    }catch(Exception e) { }
                }
                if (tank.Type == 0)
                {
                    break;
                }
                Thread.Sleep(10);
            }
        }

        //封装子弹刷新及碰撞方法
        private void bulletRefreshAndHit(Bullet bullet, Tank tank)
        {
            if (bulletBossHit(bullet, boss))
            {
                bullet.IsOver = true;
                //结束线程
                for (int i = 0; i < 4; i++)
                {
                    enemyTank[i].IsOver = false;
                }
                gameRun = false;
                MessageBox.Show("Game Over!");
            }
            if (enemyBulletWallHit(bullet, wallList) != -1)
            {
                int i = enemyBulletWallHit(bullet, wallList);
                try
                {
                    wallList.RemoveAt(i);
                }catch(Exception e) { }
                bullet.IsOver = true;
            }
            if (tank.Type == 0)
            {
                Bullet value = bulletHit(bullet, enemyBulletList);
                if (value != null)
                {
                    try
                    {
                        enemyBulletList.Remove(value);
                    }catch(Exception e) { }
                    bullet.IsOver = true;
                }
            }
            else
            {
                Bullet i = bulletHit(bullet, playerBulletList);
                if (i != null)
                {
                    try
                    {
                        playerBulletList.Remove(i);
                    }catch(Exception e) { }
                    bullet.IsOver = true;
                }
            }
        }

        //本方坦克移动
        public void myTankMove(bool upPress,bool downPress,bool leftPress,bool rightPress,Tank playerTank,List<Wall> wallList,List<Stell> stellList)
        {
            if (upPress)
            {
                playerTank.Direction = 0;
                    Rectangle rec = new Rectangle(playerTank.X, playerTank.Y - 2, playerTank.Width, playerTank.Height);
                    if (wallTankHit(rec, wallList) || stellTankHit(rec, stellList))
                    { }
                    else
                    {
                        playerTank.Y -= 2;
                    }
                    playerTank.refreshRectangle(playerTank.X, playerTank.Y);  
            }
            else if (downPress)
            {
                playerTank.Direction = 1;
                    Rectangle rec = new Rectangle(playerTank.X, playerTank.Y + 2, playerTank.Width, playerTank.Height);
                    if (wallTankHit(rec, wallList) || stellTankHit(rec, stellList))
                    { }
                    else
                    {
                        playerTank.Y += 2;
                    }
                    playerTank.refreshRectangle(playerTank.X, playerTank.Y);
            }
            else if (leftPress)
            {
                playerTank.Direction = 2;
                    Rectangle rec = new Rectangle(playerTank.X - 2, playerTank.Y, playerTank.Width, playerTank.Height);
                    if (wallTankHit(rec, wallList) || stellTankHit(rec, stellList))
                    { }
                    else
                    {
                        playerTank.X -= 2;
                    }
                    playerTank.refreshRectangle(playerTank.X, playerTank.Y);
            }
            else if (rightPress)
            {
                playerTank.Direction = 3;
                    Rectangle rec = new Rectangle(playerTank.X + 2, playerTank.Y, playerTank.Width, playerTank.Height);
                    if (wallTankHit(rec, wallList) ||stellTankHit(rec, stellList))
                    { }
                    else
                    {
                        playerTank.X += 2;
                    }
                    playerTank.refreshRectangle(playerTank.X, playerTank.Y);
            }
        }

        //玩家坦克碰撞检测
        public bool playerTankHitIntersect(Tank tank, List<Tank> enemyList,List<Bullet> bulletList)
        {
            for (int i = 0; i < enemyList.Count; i++)
            {
                if (tank.Rectangle.IntersectsWith(enemyList[i].Rectangle))
                {
                    return true;
                }
            }
            for (int i=0;i<bulletList.Count;i++)
            {
                try
                {
                    if (tank.Rectangle.IntersectsWith(bulletList[i].Rectangle))
                    {
                        return true;
                    }
                }catch(Exception e) { }
            }
            return false;
        }

        //敌方坦克之间碰撞检测
        public bool enemyTankHitIntersect(Rectangle rectangle, List<Tank> tankList, int k)
        {
            for (int i = 0; i < tankList.Count; i++)
            {
                try
                {
                    if (i != k && rectangle.IntersectsWith(tankList[i].Rectangle))
                    {
                        return true;
                    }
                }
                catch (Exception ex) { }
            }
            return false;
        }

        //敌方坦克与我方子弹碰撞检测
        public Bullet playerBulletHitEnemy(List<Bullet> bulletList, Tank tank)
        {
            for(int i=0;i<bulletList.Count;i++)
            {
                if (bulletList[i].Rectangle.IntersectsWith(tank.Rectangle))
                {
                    return bulletList[i];
                }
            }
            return null;
        }

        //子弹间碰撞检测
        public Bullet bulletHit(Bullet bullet, List<Bullet> list)
        {
            for(int i=0;i<list.Count;i++)
            {
                try
                {
                    if (bullet.Rectangle.IntersectsWith(list[i].Rectangle))
                    {
                        return list[i];
                    }
                }
                catch (Exception ex) { }
            }
            return null;
        }

        //坦克与墙的碰撞
        public bool wallTankHit(Rectangle rectangle, List<Wall> wallList)
        {
            for (int i = 0; i < wallList.Count; i++)
            {
                try
                {
                    if (rectangle.IntersectsWith(wallList[i].Rectangle))
                    {
                        return true;
                    }
                }
                catch (Exception ex) { }
            }
            return false;
        }

        //坦克与钢的碰撞
        public bool stellTankHit(Rectangle rectangle, List<Stell> stellList)
        {
            for (int i = 0; i < stellList.Count; i++)
            {
                try
                {
                    if (rectangle.IntersectsWith(stellList[i].Rectangle))
                    {
                        return true;
                    }
                }
                catch (Exception ex) { }
            }
            return false;
        }

        //生成坦克位置碰撞检测
        public bool makeTankHit(Rectangle rec, List<Rectangle> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                try
                {
                    if (rec.IntersectsWith(list[i]))
                    {
                        return true;
                    }
                }
                catch (Exception ex) { }
            }
            return false;
        }

        //敌方子弹与墙的碰撞检测,与本方分开，减少一些循环
        public int enemyBulletWallHit(Bullet bullet, List<Wall> wallList)
        {
            for (int i = 0; i < wallList.Count; i++)
            {
                try
                {
                    if (bullet.Rectangle.IntersectsWith(wallList[i].Rectangle))
                    {
                        return i;
                    }
                }
                catch (Exception ex) { }
            }
            return -1;
        }

        //敌方子弹与钢的碰撞检测
        public bool enemyBulletStellHit(Rectangle rec, List<Stell> stellList)
        {
            for (int i = 0; i < stellList.Count; i++)
            {
                try
                {
                    if (rec.IntersectsWith(stellList[i].Rectangle))
                    {
                        return true;
                    }
                }
                catch (Exception ex) { }
            }
            return false;
        }

        //子弹与大王的碰撞检测
        public bool bulletBossHit(Bullet bullet, Boss boss)
        {
            if (bullet.Rectangle.IntersectsWith(boss.Rectangle))
            {
                return true;
            }
            return false;
        }

    }
}
