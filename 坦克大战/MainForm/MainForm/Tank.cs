using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MainForm
{
    class Tank
    {
        private int width;//坦克宽
        private int height;//坦克高
        private int x;//坦克x轴坐标
        private int y;//坦克y坐标
        private int type;//坦克类型,0本方，其它敌方
        private int direction;//坦克方向,0上1下2左3右
        private Rectangle rectangle;
        private bool isOver = true;
        private bool isLaunch = false;
        int num = 0;
   

        public Tank(int x, int y, int type, int direction)
        {
            this.width = 34;
            this.height =34;
            this.x = x;
            this.y = y;
            this.type = type;
            this.direction = direction;
            Point p = new Point(x, y);
            Size size = new Size(this.width, this.height);
            rectangle = new Rectangle(p, size);
        }

        public int Y { get => y; set => y = value; }
        public int Width { get => width; set => width = value; }
        public int Height { get => height; set => height = value; }
        public int X { get => x; set => x = value; }
        public int Type { get => type; set => type = value; }
        public int Direction { get => direction; set => direction = value; }
        public Rectangle Rectangle { get => rectangle; set => rectangle = value; }
        public bool IsOver { get => isOver; set => isOver = value; }
        public bool IsLaunch { get => isLaunch; set => isLaunch = value; }
        public int Num { get => num; set => num = value; }

        public void refreshRectangle(int x,int y)
        {
            Point p = new Point(x, y);
            rectangle.Location = p;
        }
           
        
    }
}
