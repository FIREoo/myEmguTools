using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Runtime.InteropServices;
using System.Diagnostics;//trace
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Drawing;
using Point = System.Drawing.Point;
namespace myEmguLibrary
{
    public class myChart
    {
        public class Table
        {
            public Rectangle rect_innerBox = new Rectangle(0, 0, 0, 0);
            public Rectangle rect_outerBox = new Rectangle(0, 0, 0, 0);
            public int column_count = 0;
            public int row_count = 0;
            public MCvScalar color = new MCvScalar(255, 255, 255);
            public int thickness = 1;

            public Point findPoint(int col, int row)
            {
                return new Point(
                   rect_innerBox.X + (int)((double)col * (double)rect_innerBox.Width / (double)column_count),
                   rect_innerBox.Y + (int)((double)(row_count - row ) * (double)rect_innerBox.Height / (double)row_count));
            }
        }
        public class Value
        {
            public enum type
            {
                stock = 1
            }
            public int row = 0;
            public int col = 0;
            //stock
            public double Open = 0;
            public double Close = 0;
            public double High = 0;
            public double Low = 0;
        }

        public void drawTable(Mat mat, Table table)
        {
            CvInvoke.Rectangle(mat, table.rect_outerBox, table.color, table.thickness);

            for (int r = 0; r < table.row_count + 1; r++)//+1是因為要畫最後的線
            {
                CvInvoke.Line(
                    mat,
                    new Point(table.rect_innerBox.X, table.rect_innerBox.Y + (int)((double)r * (double)table.rect_innerBox.Height / (double)table.row_count)),
                    new Point(table.rect_innerBox.X + table.rect_innerBox.Width, table.rect_innerBox.Y + (int)((double)r * (double)table.rect_innerBox.Height / (double)table.row_count)),
                    table.color,
                    table.thickness);
            }
            for (int c = 0; c < table.column_count + 1; c++)//+1是因為要畫最後的線
            {
                CvInvoke.Line(
                    mat,
                    new Point(table.rect_innerBox.X + (int)((double)c * (double)table.rect_innerBox.Width / (double)table.column_count), table.rect_innerBox.Y),
                    new Point(table.rect_innerBox.X + (int)((double)c * (double)table.rect_innerBox.Width / (double)table.column_count), table.rect_innerBox.Y + table.rect_innerBox.Height),
                    table.color,
                    table.thickness);
            }
        }
        public void drawValue(Mat mat, Table table, Value value)
        {
            CvInvoke.Circle(mat, table.findPoint(value.col, value.row), 5, new MCvScalar(200, 200, 200), -1);
        }


    }
}
