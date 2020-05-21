using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace bck.SMR_simulator.draw2d
{
    class Draw2D
    {
        private Grid grid;
        internal Shapes2D shapes = new Shapes2D();

        internal BoundaryConditionMarks boundaryConditionMarks;

        public Draw2D(Grid grid)
        {
            this.grid = grid;
            boundaryConditionMarks = new BoundaryConditionMarks(shapes);
        }

        internal void RedrawShapes()
        {
            foreach (Line2D line in shapes.lines)
            {
                grid.Children.Add(line.lineObj);
            }
            foreach (Polygon2D polygon in shapes.polygons)
            {
                grid.Children.Add(polygon.polygonObj);
            }
        }
    }
    class Shapes2D : List<Shape2D>
    {
        internal Lines2D lines = new Lines2D();
        internal Polygons2D polygons = new Polygons2D();

        //internal List<Line> lines = new List<Line>();
        internal List<Rectangle> rectangles = new List<Rectangle>();

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
        public Shape2D()
        {
        }
    }
    class Lines2D : List<Line2D>
    {
        internal void Add(Point p0, Point p1)
        {
            Line2D line = new Line2D(p0, p1);
            base.Add(line);
        }
    }
    class Line2D : Shape2D
    {
        internal Line lineObj;
        private Point p0;
        private Point p1;
        internal Line2D(Point p0, Point p1)
        {
            this.p0 = p0;
            this.p1 = p1;
            lineObj = new Line
            {
                Stroke = System.Windows.Media.Brushes.Black,
                X1 = p0.X,
                X2 = p1.X,
                Y1 = p0.Y,
                Y2 = p1.Y,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                StrokeThickness = 1
            };
            //grid.Children.Add(lineObj);
        }
    }
    class Polygons2D : List<Polygon2D>
    {
        internal void Add(List<Point> points)
        {
            Polygon2D polygon = new Polygon2D(points);
            base.Add(polygon);
        }
        internal Polygon2D AddTriangle(Point p0, Point p1, Point p2)
        {
            List<Point> points = new List<Point>();
            points.Add(p0);
            points.Add(p1);
            points.Add(p2);

            Polygon2D polygon = new Polygon2D(points);
            base.Add(polygon);
            return polygon;
        }
    }
    class Polygon2D : Shape2D
    {
        internal Polygon polygonObj;
        private List<Point> points;
        internal Brush Stroke
        {
            get
            {
                return polygonObj.Stroke;
            }
            set
            {
                polygonObj.Stroke = value;
            }
        }
        internal double StrokeThickness
        {
            get
            {
                return polygonObj.StrokeThickness;
            }
            set
            {
                polygonObj.StrokeThickness = value;
            }
        }
        internal Brush Fill
        {
            get
            {
                return polygonObj.Fill;
            }
            set
            {
                polygonObj.Fill = value;
            }
        }
        internal Polygon2D(List<Point> points)
        {
            this.points = points;

            PointCollection myPointCollection = new PointCollection();
            foreach (Point point in points)
            {
                myPointCollection.Add(point);
            }

            polygonObj = new Polygon();
            polygonObj.Points = myPointCollection;

            polygonObj.Stroke = Brushes.Black;
            polygonObj.StrokeThickness = 0.5;
            polygonObj.Fill = Brushes.Yellow;
            //polygonObj.Width = 100;
            //polygonObj.Height = 100;
            //polygonObj.Stretch = Stretch.Fill;
        }
    }
    class BoundaryConditionMarks : List<BoundaryConditionMark>
    {
        private Shapes2D baseShapes;

        public BoundaryConditionMarks(Shapes2D baseShapes)
        {
            this.baseShapes = baseShapes;
        }
        internal new void Clear()
        {
            foreach (BoundaryConditionMark boundaryConditionMark in this)
            {
                foreach (Polygon2D polygon in boundaryConditionMark.shapes.polygons)
                {
                    baseShapes.polygons.Remove(polygon);
                }
            }
            base.Clear();
        }
        internal void Add(Point p0, int[] boundaryCondition)
        {
            BoundaryConditionMark boundaryConditionMark = new BoundaryConditionMark(p0, boundaryCondition);
            Add(boundaryConditionMark);
            foreach (Polygon2D polygon in boundaryConditionMark.shapes.polygons)
            {
                baseShapes.polygons.Add(polygon);
            }
        }
    }
    class BoundaryConditionMark : Shape2D
    {
        private Point p0;
        private double radius = 10;
        private int[] boundaryCondition = new int[6];
        internal Shapes2D shapes = new Shapes2D();

        public BoundaryConditionMark(Point p0, int[] boundaryCondition)
        {
            this.p0 = p0;
            this.boundaryCondition = boundaryCondition;

            Point[] points = new Point[6];

            double x, y, angle;
            for (int i = 0; i < 6; i++)
            {
                angle = Math.PI /2 - Math.PI / 3 * i;
                x = p0.X + radius * Math.Cos(angle);
                y = p0.Y - radius * Math.Sin(angle);
                points[i].X = x;
                points[i].Y = y;
            }

            for (int i = 0; i < 6; i++)
            {
                Polygon2D polygon = shapes.polygons.AddTriangle(p0, points[i], points[(i + 1) % 6]);
                polygon.Stroke = Brushes.Black;
                polygon.StrokeThickness = 0.5;
                if (boundaryCondition[i] == 0)
                {
                    polygon.Fill = Brushes.Yellow;
                }
                else
                {
                    polygon.Fill = Brushes.Red;
                }
            }
        }

    }
    class SelectionWindow
    {
        private readonly Grid grid;
        internal bool enable = false;
        internal bool started = false;

        internal Shapes2D shapes = new Shapes2D();
        internal Point wP0, wP1;
        public ViewType viewType;
        public enum ViewType
        {
            SelectionWindow,
            Rectangle,
            Line,
            Arrow,
            Circle,
            Cross
        }
        Rectangle rectangle;
        Line line;
        Line crossLine0, crossLine1;

        /// <summary>
        /// 커서로 사용할 십자가모양의 반지름 길이 설정.
        /// </summary>
        private double crossRadius = 10;
        double crossLineStrokeThickness = 1;


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
                case ViewType.Cross:
                    DrawCross();
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
                case ViewType.Cross:
                    ChangeCross();
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
                case ViewType.Cross:
                    grid.Children.Remove(crossLine0);
                    grid.Children.Remove(crossLine1);
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
                Y2 = wP1.Y,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                StrokeThickness = 1
            };
            grid.Children.Add(line);
        }
        private void DrawCross()
        {
            Point center = wP0;
            Point[] points = GetCrossPoints(center);
            crossLine0 = new Line
            {
                Stroke = System.Windows.Media.Brushes.Black,
                X1 = points[0].X,
                X2 = points[2].X,
                Y1 = points[0].Y,
                Y2 = points[2].Y,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                StrokeThickness = crossLineStrokeThickness
            };
            crossLine1 = new Line
            {
                Stroke = System.Windows.Media.Brushes.Black,
                X1 = points[1].X,
                X2 = points[3].X,
                Y1 = points[1].Y,
                Y2 = points[3].Y,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                StrokeThickness = crossLineStrokeThickness
            };
            grid.Children.Add(crossLine0);
            grid.Children.Add(crossLine1);
        }
        /// <summary>
        /// <param name="center"/>를 중심으로 십자가를 표현하는 시계방향순서의 4개 절점 반환.
        /// </summary>
        /// <param name="center"></param>
        /// <returns></returns>
        private Point[] GetCrossPoints(Point center)
        {
            Point[] points = new Point[4];
            points[0] = center + new Vector(0, -crossRadius);
            points[1] = center + new Vector(crossRadius, 0);
            points[2] = center + new Vector(0, crossRadius);
            points[3] = center + new Vector(-crossRadius, 0);
            return points;
        }

        private void ChangeLine()
        {
            line.X2 = wP1.X;
            line.Y2 = wP1.Y;
        }
        private void ChangeCross()
        {
            Point center = wP1;
            Point[] points = GetCrossPoints(center);

            crossLine0.X1 = points[0].X;
            crossLine0.Y1 = points[0].Y;
            crossLine0.X2 = points[2].X;
            crossLine0.Y2 = points[2].Y;
            crossLine1.X1 = points[1].X;
            crossLine1.Y1 = points[1].Y;
            crossLine1.X2 = points[3].X;
            crossLine1.Y2 = points[3].Y;
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
