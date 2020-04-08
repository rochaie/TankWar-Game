using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainForm
{
    class Bullet
    {
        private int width;//子弹宽
        private int height;//子弹高
        private int x;//子弹x轴位置
        private int y;//子弹y轴位置
        private Rectangle rectangle;
        int direction = 0;
        bool isOver;

        public Bullet( int x, int y)
        {
            this.width =12;
            this.height = 20;
            this.x = x;
            this.y = y;
            Point p = new Point(x, y);
            Size size = new Size(this.width, this.height);
            Rectangle = new Rectangle(p, size);
        }

        public int Width { get => width; set => width = value; }
        public int Height { get => height; set => height = value; }
        public int X { get => x; set => x = value; }
        public int Y { get => y; set => y = value; }
        public Rectangle Rectangle { get => rectangle; set => rectangle = value; }
        public int Direction { get => direction; set => direction = value; }
        public bool IsOver { get => isOver; set => isOver = value; }

        public void refreshRectangle(int x, int y)
        {
            Point p = new Point(x, y);
            rectangle.Location = p;
        }
    }
}
