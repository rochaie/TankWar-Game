using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainForm
{
    class Wall
    {
        private int x;//墙x坐标
        private int y;//墙y坐标
        private int width;//墙宽度
        private int height;//墙高度
        private Rectangle rectangle;

        public Wall(int x, int y)
        {
            this.x = x;
            this.y = y;
            this.width = 20;
            this.height = 20;
            Rectangle rec = new Rectangle(x, y, 20, 20);
            this.Rectangle = rec;
        }

        public int X { get => x; set => x = value; }
        public int Y { get => y; set => y = value; }
        public int Width { get => width; set => width = value; }
        public int Height { get => height; set => height = value; }
        public Rectangle Rectangle { get => rectangle; set => rectangle = value; }
    }
}
