using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainForm
{
    class Grass
    {
        private int x;
        private int y;
        private int width;
        private int height;
        private Rectangle rectangle;

        public Grass(int x, int y,int width,int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height =height;
            Rectangle rec = new Rectangle(x, y, width, height);
            this.Rectangle = rec;
        }

        public int X { get => x; set => x = value; }
        public int Y { get => y; set => y = value; }
        public int Width { get => width; set => width = value; }
        public int Height { get => height; set => height = value; }
        public Rectangle Rectangle { get => rectangle; set => rectangle = value; }
    }
}
