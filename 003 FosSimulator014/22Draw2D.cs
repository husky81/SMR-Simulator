using _Draw3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace _Draw2D
{
    class Draw2D
    {
        private Grid grid;
        public Shapes2D shapes = new Shapes2D();

        public Draw2D(Grid grid)
        {
            this.grid = grid;   
        }
    }
    class Shapes2D : List<Shape2D>
    {
        internal Lines2D lines = new Lines2D();

        //internal List<Line> lines = new List<Line>();
        //internal List<Rectangle> rectangles = new List<Rectangle>();



        internal void AddRectangle(Point strPoint, Point endPoint)
        {
            Rectangle r = new Rectangle();
            //r.PointFromScreen(strPoint);
            //r.PointToScreen(endPoint);
            rectangles.Add(r);
        }
    }
    class Shape2D
    {

    }
    class Lines2D : List<Line2D>
    {

    }
    class Line2D : Shape2D
    {
        private Point p0;
        private Point p1;
    }
    class SelectionWindow
    {
        private readonly Grid grid;
        internal bool enable = false;
        internal Shapes2D shapes = new Shapes2D();
        internal Point wP0, wP1;
        public ViewType viewType;
        public enum ViewType
        {
            SelectionWindow,
            Rectangle,
            Line,
            Arrow,
            Circle
        }
        Rectangle rectangle;
        Line line;
        internal bool started = false;

        public SelectionWindow(Grid grid)
        {
            this.grid = grid;
            //shapes.AddRectangle(strPoint, endPoint);
        }
        internal void Start(Point strPoint)
        {
            started = true;
            this.wP0 = strPoint;

            switch (viewType)
            {
                case ViewType.SelectionWindow:
                    DrawSelectionWindow();
                    break;
                case ViewType.Rectangle:
                    DrawRectangle();
                    break;
                case ViewType.Line:
                    if (line != null) grid.Children.Remove(line);
                    DrawLine();
                    break;
                case ViewType.Arrow:
                    break;
                case ViewType.Circle:
                    break;
                default:
                    break;
            }
        }
        internal void Move(Point endPoint)
        {
            if (!started) return;
            this.wP1 = endPoint;
            switch (viewType)
            {
                case ViewType.SelectionWindow:
                    ChangeSelectionWindow();
                    break;
                case ViewType.Rectangle:
                    ChangeRectangle();
                    break;
                case ViewType.Line:
                    ChangeLine();
                    break;
                case ViewType.Arrow:
                    break;
                case ViewType.Circle:
                    break;
                default:
                    break;
            }
        }
        internal void End()
        {
            started = false;
            switch (viewType)
            {
                case ViewType.SelectionWindow:
                    grid.Children.Remove(rectangle);
                    break;
                case ViewType.Rectangle:
                    grid.Children.Remove(rectangle);
                    break;
                case ViewType.Line:
                    grid.Children.Remove(line);
                    //grid.Children.Clear();
                    break;
                case ViewType.Arrow:
                    break;
                case ViewType.Circle:
                    break;
                default:
                    break;
            }
        }
        private void DrawLine()
        {
            line = new Line
            {
                Stroke = System.Windows.Media.Brushes.Black,
                X1 = wP0.X,
                X2 = wP1.X,
                Y1 = wP0.Y,
                Y2 = wP0.Y,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                StrokeThickness = 1
            };
            grid.Children.Add(line);
        }
        private void ChangeLine()
        {
            line.X2 = wP1.X;
            line.Y2 = wP1.Y;
        }
        private void DrawSelectionWindow()
        {
            rectangle = new Rectangle();
            rectangle.Width = 0;
            rectangle.Height = 0;
            rectangle.Fill = Brushes.Blue;
            rectangle.Opacity = 0.2;
            rectangle.Margin = new Thickness(wP0.X, wP0.Y, 0, 0);
            rectangle.Stroke = Brushes.Black;
            rectangle.HorizontalAlignment = HorizontalAlignment.Left;
            rectangle.VerticalAlignment = VerticalAlignment.Top;
            grid.Children.Add(rectangle);
        }
        private void DrawRectangle()
        {
            rectangle = new Rectangle();
            rectangle.Width = 0;
            rectangle.Height = 0;
            rectangle.Fill = null;
            rectangle.Opacity = 1;
            rectangle.Margin = new Thickness(wP0.X, wP0.Y, 0, 0);
            rectangle.Stroke = Brushes.Black;
            rectangle.HorizontalAlignment = HorizontalAlignment.Left;
            rectangle.VerticalAlignment = VerticalAlignment.Top;
            grid.Children.Add(rectangle);
        }
        private void ChangeRectangle()
        {
            double top = wP0.Y;
            double left = wP0.X;
            double width = wP1.X - wP0.X;
            double height = wP1.Y - wP0.Y;

            if (height < 0)
            {
                height = -height;
                top -= height;
            }

            if (width < 0)
            {
                width = -width;
                left -= width;
            }
            rectangle.Margin = new Thickness(left, top, 0, 0);
            rectangle.Width = width;
            rectangle.Height = height;
        }
        private void ChangeSelectionWindow()
        {
            double top = wP0.Y;
            double left = wP0.X;
            double width = wP1.X - wP0.X;
            double height = wP1.Y - wP0.Y;

            if (height < 0)
            {
                height = -height;
                top -= height;
            }

            if (width < 0)
            {
                width = -width;
                left -= width;
                rectangle.Fill = Brushes.Green;
                rectangle.StrokeDashArray = new DoubleCollection() { 4, 4 };
            }
            else
            {
                rectangle.Fill = Brushes.Blue;
                rectangle.StrokeDashArray = new DoubleCollection();
            }
            rectangle.Margin = new Thickness(left, top, 0, 0);
            rectangle.Width = width;
            rectangle.Height = height;
        }
    }
}
