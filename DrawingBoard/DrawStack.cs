using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DrawingBoard
{
    class DrawStack
    {
        public class Data
        {
            public Shape shape { get; set; }
            public Point location1 { get; set; }
            public Point location2 { get; set; }
            public string type { get; set; }
            public Brush fillColor { get; set; }
            public Brush lineColor { get; set; }
            public double lineStroke { get; set; }
        }
    }
}
