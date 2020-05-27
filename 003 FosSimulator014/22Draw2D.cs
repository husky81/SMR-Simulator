using BCK.SmrSimulation.Draw3D;
using BCK.SmrSimulation.GeneralFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace BCK.SmrSimulation.Draw2D
{
    class Draw2D
    {
        private Grid grid;

        internal Shape2dCollection shapes = new Shape2dCollection();
        internal Text2dCollection texts = new Text2dCollection();

        internal BoundaryConditionMarks boundaryConditionMarks;
        internal ObjectSnapMark objectSnapMark;
        public Draw2D(Grid grid)
        {
            this.grid = grid;
            boundaryConditionMarks = new BoundaryConditionMarks(shapes);
            objectSnapMark = new ObjectSnapMark(grid);
        }

        internal void RedrawShapes()
        {
            foreach (Line2D line in shapes.lines)
            {
                if (line.IsOnGridArea)
                {
                    grid.Children.Add(line.object_);
                }
            }
            foreach (Polygon2D polygon in shapes.polygons)
            {
                if (polygon.IsOnGridArea)
                {
                    grid.Children.Add(polygon.object_Polygon);
                }
            }
            foreach (Text2D text in texts)
            {
                if (text.IsOnGrid)
                {
                    grid.Children.Add(text.textObj);
                }
            }
        }

    }

    class Shape2dCollection : List<Shape2D>
    {
        internal UIElement recentObject;

        internal Line2dCollection lines = new Line2dCollection();
        internal Polygon2dCollection polygons = new Polygon2dCollection();
        internal Rectangle2dCollection rectangles = new Rectangle2dCollection();

        public Shape2dCollection()
        {
        }

        internal new void Clear()
        {
            if (grid != null)
            {
                foreach (Shape2D shape in this)
                {
                    grid.Children.Remove(shape.object_);
                }
            }
            base.Clear();
            lines.Clear();
            polygons.Clear();
            rectangles.Clear();
        }

        internal Shape2dCollection All
        {
            get
            {
                base.Clear();
                base.AddRange(lines);
                base.AddRange(polygons);
                base.AddRange(rectangles);
                return this;
            }
        }

        internal Rectangle2D AddSquare(Point center, double size)
        {
            return rectangles.Add(center, size);
        }

        internal void DrawAtGrid(Grid grid)
        {
            this.grid = grid;
            foreach (Shape2D shape in All)
            {
                grid.Children.Add(shape.object_);
            }
        }
        private Grid grid;

        internal Shape2D AddTriangleReqular(Point center, double radius)
        {
            Polygon2D polgon2D = polygons.AddTriangleRegular(center, radius);
            return polgon2D;
        }
    }
    class Shape2D
    {
        internal List<Point> points;
        internal Shape object_;

        public Shape2D()
        {

        }
        internal bool IsOnGridArea
        {
            get
            {
                foreach (Point point in points)
                {
                    if (point.X < 0 | point.Y < 0) return false;
                }
                return true;
            }
        }

        internal SolidColorBrush Color
        {
            set
            {
                object_.Stroke = value;
            }
        }
        internal double Thickness
        {
            set
            {
                object_.StrokeThickness = value;
            }
        }
    }
    class Line2dCollection : List<Line2D>
    {
        internal Line2D Add(Point p0, Point p1)
        {
            Line2D line = new Line2D(p0, p1);
            base.Add(line);
            return line;
        }
    }
    class Line2D : Shape2D
    {
        internal Line2D(Point p0, Point p1)
        {
            points = new List<Point>();
            points.Add(p0);
            points.Add(p1);
            object_ = new Line
            {
                Stroke = System.Windows.Media.Brushes.Black,
                X1 = points[0].X,
                X2 = points[1].X,
                Y1 = points[0].Y,
                Y2 = points[1].Y,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                StrokeThickness = 1
            };
            //grid.Children.Add(lineObj);
        }
    }
    class Rectangle2D : Shape2D
    {
        private Point center;
        private double size;

        public Rectangle2D(Point center, double size)
        {
            this.center = center;
            this.size = size;

            object_ = new Rectangle
            {
                Stroke = Brushes.Black,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Height = size,
                Width = size,
                Margin = new Thickness(center.X - size / 2, center.Y - size / 2,0,0),
            };
        }

    }
    class Rectangle2dCollection : List<Rectangle2D>
    {
        internal Rectangle2D Add(Point center, double size)
        {
            Rectangle2D rectangle = new Rectangle2D(center, size);
            base.Add(rectangle);
            return rectangle;
        }
    }
    class Polygon2dCollection : List<Polygon2D>
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

        internal Polygon2D AddTriangleRegular(Point center, double radius)
        {
            Point p0 = new Point(center.X + radius * Math.Sin( 60.0/180.0*Math.PI), center.Y + radius * Math.Cos( 60.0/180.0*Math.PI));
            Point p1 = new Point(center.X + radius * Math.Sin(180.0/180.0*Math.PI), center.Y + radius * Math.Cos(180.0/180.0*Math.PI));
            Point p2 = new Point(center.X + radius * Math.Sin(300.0/180.0*Math.PI), center.Y + radius * Math.Cos(300.0 / 180.0*Math.PI));
            
            return AddTriangle(p0, p1, p2);
        }
    }
    class Polygon2D : Shape2D
    {
        internal Polygon object_Polygon;

        internal Brush Stroke
        {
            get
            {
                return object_Polygon.Stroke;
            }
            set
            {
                object_Polygon.Stroke = value;
            }
        }
        internal double StrokeThickness
        {
            get
            {
                return object_Polygon.StrokeThickness;
            }
            set
            {
                object_Polygon.StrokeThickness = value;
            }
        }
        internal Brush Fill
        {
            get
            {
                return object_Polygon.Fill;
            }
            set
            {
                object_Polygon.Fill = value;
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

            object_Polygon = new Polygon();
            object_Polygon.Points = myPointCollection;

            object_Polygon.Stroke = Brushes.Black;
            object_Polygon.StrokeThickness = 0.5;
            //object_Polygon.Fill = Brushes.Yellow;
            //polygonObj.Width = 100;
            //polygonObj.Height = 100;
            //polygonObj.Stretch = Stretch.Fill;
            base.object_ = object_Polygon;
        }
    }
    class BoundaryConditionMarks : List<BoundaryConditionMark>
    {
        private Shape2dCollection baseShapes;

        public BoundaryConditionMarks(Shape2dCollection baseShapes)
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
        internal Shape2dCollection shapes = new Shape2dCollection();

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
    class ObjectSnapMark
    {
        private Grid grid;
        internal Shape2dCollection shapes = new Shape2dCollection();

        private double thickness = 2.5;
        private double markSize = 12;
        private SolidColorBrush color = Brushes.DarkOrange;

        public ObjectSnapMark(Grid grid)
        {
            this.grid = grid;
        }
        internal void Clear()
        {
            shapes.Clear();
        }
        internal void Draw(ObjectSnapPoint objectSnapPoint)
        {
            Clear();
            if (objectSnapPoint == null) return;

            Point center = objectSnapPoint.point2d;
            Shape2D shape = new Shape2D();

            switch (objectSnapPoint.snapType)
            {
                case ObjectSnapPoint.Types.End:
                case ObjectSnapPoint.Types.Node:
                    double size = markSize;
                    shape = shapes.AddSquare(center, size);
                    break;
                case ObjectSnapPoint.Types.Mid:
                    double radius = markSize / 2;
                    shape = shapes.AddTriangleReqular(center, radius);
                    break;
                case ObjectSnapPoint.Types.Center:
                    break;
                default:
                    break;
            }

            if (shape == null) return;
            shape.Color = color;
            shape.Thickness = thickness;
            objectSnapPoint.object_ = shape.object_;

            shapes.DrawAtGrid(grid);
        }
    }

    class Text2dCollection : List<Text2D>
    {
        internal Text2D Add(Point p0, String text)
        {
            Text2D textObj = new Text2D(p0, text, 12);
            base.Add(textObj);
            return textObj;
        }
        internal Text2D Add(Point p0, String text, int size)
        {
            Text2D textObj = new Text2D(p0, text, size);
            base.Add(textObj);
            return textObj;
        }
    }
    class Text2D
    {
        internal TextBlock textObj; //TextBlock이 Label보다 훨씬 가볍다. ref. https://m.blog.naver.com/PostView.nhn?blogId=inasie&logNo=70025582628&proxyReferer=http:%2F%2Fwww.google.com%2Furl%3Fsa%3Dt%26rct%3Dj%26q%3D%26esrc%3Ds%26source%3Dweb%26cd%3D%26ved%3D2ahUKEwjT5b7E8cTpAhVryosBHTHRC5YQFjAJegQIDBAB%26url%3Dhttp%253A%252F%252Fm.blog.naver.com%252Finasie%252F70025582628%26usg%3DAOvVaw3TmFXvkGjN0Y_VDPh11e20

        public Point Point { get => point; 
            set
            {
                point = value;
                drawingPoint = value;
            }
        }
        private Point point;
        Point drawingPoint;
        String text;
        public Text2D(Point point, String text, int size)
        {
            this.Point = point;
            this.text = text;
            textObj = new TextBlock();
            textObj.FontSize = size;
            textObj.Text = text;
            textObj.HorizontalAlignment = HorizontalAlignment.Left;
            textObj.VerticalAlignment = VerticalAlignment.Top;
            textObj.Foreground = Brushes.Black;

            try
            {
                textObj.Margin = new Thickness(point.X, point.Y, 0, 0);
            }
            catch (Exception)
            {
                if(point.X.Equals(double.NaN))
                {
                    
                }
            }
        }

        internal enum Allignments
        {
            topLeft,
            topCenter,
            topRight,
            middleLeft,
            middleCenter,
            middleRight,
            bottomLeft,
            bottomCenter,
            bottomRight,
        }
        private Allignments allignment = Allignments.topLeft;
        internal Allignments Allignment
        {
            get
            {
                return allignment;
            }
            set
            {
                if(value != allignment)
                {
                    allignment = value;
                    Size size = GF.MeasureString(text, textObj);
                    double h = size.Height;
                    double w = size.Width;

                    switch (value)
                    {
                        case Allignments.topLeft:
                            drawingPoint.Y = Point.Y;
                            drawingPoint.X = Point.X;
                            break;
                        case Allignments.topCenter:
                            drawingPoint.Y = Point.Y;
                            drawingPoint.X = Point.X - w / 2;
                            break;
                        case Allignments.topRight:
                            drawingPoint.Y = Point.Y;
                            drawingPoint.X = Point.X - w;
                            break;
                        case Allignments.middleLeft:
                            drawingPoint.Y = Point.Y - h / 2;
                            drawingPoint.X = Point.X;
                            break;
                        case Allignments.middleCenter:
                            drawingPoint.Y = Point.Y - h / 2;
                            drawingPoint.X = Point.X - w / 2;
                            break;
                        case Allignments.middleRight:
                            drawingPoint.Y = Point.Y - h / 2;
                            drawingPoint.X = Point.X - w;
                            break;
                        case Allignments.bottomLeft:
                            drawingPoint.Y = Point.Y - h;
                            drawingPoint.X = Point.X;
                            break;
                        case Allignments.bottomCenter:
                            drawingPoint.Y = Point.Y - h;
                            drawingPoint.X = Point.X - w / 2;
                            break;
                        case Allignments.bottomRight:
                            drawingPoint.Y = Point.Y - h;
                            drawingPoint.X = Point.X - w;
                            break;
                        default:
                            break;
                    }
                    textObj.Margin = new Thickness(drawingPoint.X, drawingPoint.Y, 0, 0);
                }
            }
        }

        public Brush Color { get => color;
            internal set
            {
                color = value;
                textObj.Foreground = value;
            }
        }
        private Brush color = Brushes.Black;

        internal bool IsOnGrid
        {
            get
            {
                if(Point.X<0 | Point.Y < 0)
                {
                    return false;
                }
                return true;
            }
        }

    }

    class MouseInputGuideShapes
    {
        private readonly Grid grid;
        internal bool enable = false;
        internal bool started = false;

        internal Shape2dCollection shapes = new Shape2dCollection();
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

        public MouseInputGuideShapes(Grid grid)
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
                    this.wP1 = strPoint;
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
