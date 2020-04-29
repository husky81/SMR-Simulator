using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;
namespace _003_FosSimulator014
{
    public class Shapes : List<Shape>
    {
        public Model3DGroup modelGroup = new Model3DGroup();
        private Shape recentShape;

        public TranslateTransform3D transform;

        internal Shape RecentShape { get => recentShape; set => recentShape = value; }

        public Shapes()
        {
            
        }

        private new Shape Add(Shape shape)
        {
            base.Add(shape);
            RecentShape = shape;
            return shape;
        }

        internal Triangle AddTriangle(Point3D p0, Point3D p1, Point3D p2)
        {
            Triangle t = new Triangle(p0, p1, p2);
            Add(t);
            return t;
        }
        internal Line3D AddLine(Point3D sp, Point3D ep)
        {
            Line3D l = new Line3D(sp, ep);
            Add(l);
            return l;
        }
        internal Hexahedron AddBox(Point3D point, Vector3D vector)
        {
            Hexahedron b = new Hexahedron(point, vector);
            Add(b);
            return b;
        }
        internal Hexahedron AddCube(Point3D point, double size)
        {
            Hexahedron b = new Hexahedron(point, size);
            Add(b);
            return b;
        }
        internal Hexahedron AddHexahedron(Point3D p0, Point3D p1, Point3D p2, Point3D p3, Point3D p4, Point3D p5, Point3D p6, Point3D p7)
        {
            Hexahedron b = new Hexahedron(p0, p1, p2, p3, p4, p5, p6, p7);
            Add(b);
            return b;
        }
        internal Circle AddCircle(double radius, Vector3D normal, Point3D center, int resolution)
        {
            Circle c = new Circle(radius, normal, center, resolution);
            Add(c);
            return c;
        }
        internal Cone AddCone(double radius, Vector3D heightVector, Point3D center, int resolution)
        {
            Cone c = new Cone(radius, heightVector, center, resolution);
            Add(c);
            return c;
        }
        internal Cylinder AddCylinder(Point3D str, Vector3D dir, double dia, int resolution)
        {
            Cylinder c = new Cylinder(str, dir, dia, resolution);
            Add(c);
            return c;
        }
        internal Cylinder AddCylinderClosed(Point3D str, Vector3D dir, double dia, int resolution)
        {
            Cylinder s = new Cylinder(str, dir, dia, resolution);
            s.Close();
            Add(s);
            return s;
        }
        internal Arrow AddArrow(Point3D str, Vector3D dir, double dia, int resolution)
        {
            Arrow s = new Arrow(str, dir, dia, resolution);
            Add(s);
            return s;
        }
        internal Arrow AddForce(Point3D target, Vector3D force)
        {
            double dia = force.Length / 10;
            int resolution = 12;
            Arrow s = new Arrow(target - force, force, dia, resolution);
            Add(s);
            return s;
        }
        internal Sphere AddSphere(Point3D point, double diameter, int resolution)
        {
            Sphere s = new Sphere(point, diameter, resolution);
            Add(s);
            return s;
        }

        internal Polygon AddPolygon(Point3D str, Vector3D dir, SectionPoly poly)
        {
            Polygon s = new Polygon(str, dir, poly);
            Add(s);
            return s;

        }

        internal Model3DGroup Model3DGroup()
        {
            Model3DGroup model3DGroup = new Model3DGroup();
            foreach (Shape shape in this)
            {
                model3DGroup.Children.Add(shape.GeoModel());
            }
            model3DGroup.Transform = transform;
            return model3DGroup;
        }

        internal ModelVisual3D ModelVisual3D()
        {
            ModelVisual3D modelVisual3D = new ModelVisual3D();
            modelVisual3D.Content = this.Model3DGroup();
            return modelVisual3D;
        }

        internal Point3D Center()
        {
            Point3D cp = new Point3D(0, 0, 0);
            if (this.Count == 0) return cp;
            foreach (Shape shape in this)
            {
                cp.X += shape.BasePoint.X;
                cp.Y += shape.BasePoint.Y;
                cp.Z += shape.BasePoint.Z;
            }
            cp.X /= Count;
            cp.Y /= Count;
            cp.Z /= Count;
            return cp;
        }
    }
    public class Shape
    {
        internal MeshGeometry3D mesh;
        private Color color;
        private double opacity;
        public Material material;
        public RotateTransform3D rotateTransform3D = new RotateTransform3D();
        public RotateTransform3D rotateTransform3dZ = new RotateTransform3D();
        public RotateTransform3D rotateTransform3dY = new RotateTransform3D();
        public RotateTransform3D rotateTransform3dZ2 = new RotateTransform3D();
        public TranslateTransform3D translateTransform3D = new TranslateTransform3D();
        private GeometryModel3D geoModel;

        private Point3D basePoint;
        public Point3D BasePoint
        {
            get
            {
                return basePoint;
            }
            set
            {
                basePoint = value;
                SetTransforms(basePoint, direction);
            }
        }
        private Vector3D direction;
        public Shape()
        {
            color = Colors.Blue;
            opacity = 1;
            material = new DiffuseMaterial(new SolidColorBrush(color));
        }
        public GeometryModel3D GeoModel()
        {
            Transform3DGroup trn = new Transform3DGroup();
            //trn.Children.Add(rotateTransform3D);

            trn.Children.Add(rotateTransform3dZ);
            trn.Children.Add(rotateTransform3dY);
            trn.Children.Add(rotateTransform3dZ2);
            trn.Children.Add(translateTransform3D);

            geoModel = new GeometryModel3D(mesh, material);
            geoModel.Transform = trn;

            return geoModel;
        }
        public void Color(Color color)
        {
            this.color = color;
            SolidColorBrush strokeBrush = new SolidColorBrush(color);
            strokeBrush.Opacity = opacity;
            material = new DiffuseMaterial(strokeBrush);
        }
        public void Opacity(double opacity)
        {
            this.opacity = opacity;
            SolidColorBrush strokeBrush = new SolidColorBrush(color);
            strokeBrush.Opacity = opacity;
            material = new DiffuseMaterial(strokeBrush);
        }
        public void SetTransforms(Point3D basePoint, Vector3D direction)
        {
            this.basePoint = basePoint;
            this.direction = direction;

            //Vector3D normal = dir;
            //normal.Normalize();

            Vector3D axisY = new Vector3D(0, 1, 0);
            Vector3D axisZ = new Vector3D(0, 0, 1);
            double angleZ = 90;
            double angleY = 90;
            double angleZ2;
            angleZ2 = Math.Atan2(direction.Y, direction.X) * 180 / Math.PI;
            angleY -= Math.Atan2(direction.Z, Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y)) * 180 / Math.PI;

            rotateTransform3dZ = new RotateTransform3D(new AxisAngleRotation3D(axisZ, angleZ));
            rotateTransform3dY = new RotateTransform3D(new AxisAngleRotation3D(axisY, angleY));
            rotateTransform3dZ2 = new RotateTransform3D(new AxisAngleRotation3D(axisZ, angleZ2));
            translateTransform3D = new TranslateTransform3D(new Vector3D(basePoint.X, basePoint.Y, basePoint.Z));
        }

        internal void Move(Vector3D moveVector)
        {
            basePoint += moveVector;
            SetTransforms(basePoint, direction);
        }
    }
    class Triangle : Shape
    {
        public Point3D p0;
        public Point3D p1;
        public Point3D p2;
        public Triangle(Point3D p0, Point3D p1, Point3D p2)
        {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;

            mesh = MeshGenerator.Triangle(p0, p1, p2);
        }
    }
    class Line3D : Shape
    {
        public Point3D p0;
        public Point3D p1;
        public Line3D(Point3D p0, Point3D p1)
        {
            this.p0 = p0;
            this.p1 = p1;

            Point3D str = p0;
            Vector3D dir = new Vector3D(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);

            mesh = MeshGenerator.Line3D(dir.Length);
            SetTransforms(p0, dir);
        }
    }
    class Hexahedron : Shape
    {
        Point3D p0;
        Point3D p1;
        Point3D p2;
        Point3D p3;
        Point3D p4;
        Point3D p5;
        Point3D p6;
        Point3D p7;

        public Hexahedron(Point3D point, Vector3D vector)
        {
            double dx = vector.X;
            double dy = vector.Y;
            double dz = vector.Z;

            p0 = new Point3D(point.X, point.Y, point.Z);
            p1 = new Point3D(point.X + dx, point.Y, point.Z);
            p2 = new Point3D(point.X + dx, point.Y + dy, point.Z);
            p3 = new Point3D(point.X, point.Y + dy, point.Z);
            p4 = new Point3D(point.X, point.Y, point.Z + dz);
            p5 = new Point3D(point.X + dx, point.Y, point.Z + dz);
            p6 = new Point3D(point.X + dx, point.Y + dy, point.Z + dz);
            p7 = new Point3D(point.X, point.Y + dy, point.Z + dz);

            mesh = MeshGenerator.Hexahedron(p0, p1, p2, p3, p4, p5, p6, p7);
        }

        public Hexahedron(Point3D point, double size)
        {
            p0 = new Point3D(0, 0, 0);
            p1 = new Point3D(size, 0, 0);
            p2 = new Point3D(size, size, 0);
            p3 = new Point3D(0, size, 0);
            p4 = new Point3D(0, 0, size);
            p5 = new Point3D(size, 0, size);
            p6 = new Point3D(size, size, size);
            p7 = new Point3D(0, size, size);

            mesh = MeshGenerator.Hexahedron(p0, p1, p2, p3, p4, p5, p6, p7);
        }

        public Hexahedron(Point3D p0, Point3D p1, Point3D p2, Point3D p3, Point3D p4, Point3D p5, Point3D p6, Point3D p7)
        {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
            this.p4 = p4;
            this.p5 = p5;
            this.p6 = p6;
            this.p7 = p7;

            mesh = MeshGenerator.Hexahedron(p0, p1, p2, p3, p4, p5, p6, p7);
        }
    }
    class Cone : Shape
    {
        private double radius;
        private Vector3D heightVector;
        private Point3D center;
        private int resolution;
        double height;

        public Cone(double radius, Vector3D heightVector, Point3D center, int resolution)
        {
            this.radius = radius;
            this.heightVector = heightVector;
            height = heightVector.Length;
            this.center = center;
            this.resolution = resolution;

            mesh = MeshGenerator.Cone(radius, heightVector.Length, resolution);

            SetTransforms(center, heightVector);
        }
    }
    class Circle : Shape
    {
        private double radius;
        private Vector3D normal;
        private Point3D center;
        private int resolution;

        public Circle(double radius, Vector3D normal, Point3D center, int resolution)
        {
            this.radius = radius;
            this.normal = normal;
            this.center = center;
            this.resolution = resolution;

            mesh = MeshGenerator.Circle(radius, resolution);

            SetTransforms(center, normal);
        }
    }
    class Cylinder : Shape
    {
        private Point3D str;
        private Vector3D dir;
        private double dia;
        private int resolution;
        private bool closed;

        public Cylinder(Point3D str, Vector3D dir, double dia, int resolution)
        {
            this.str = str;
            this.dir = dir;
            this.dia = dia;
            this.resolution = resolution;
            closed = false;

            mesh = MeshGenerator.Cylinder(dia, dir.Length, resolution);
            Vector3D up = new Vector3D(0, 0, 1);
            SetTransforms(str, dir);
        }
        internal void Close()
        {
            closed = true;
            for (int i = 0; i < resolution; i++)
            {
                int a = 0;
                int b = i + 1;
                int c = (i < (resolution - 1)) ? i + 2 : 1;
                int d = a + resolution + 1;
                int e = b + resolution + 1;
                int f = c + resolution + 1;

                mesh.TriangleIndices.Add(a);
                mesh.TriangleIndices.Add(b);
                mesh.TriangleIndices.Add(c);
                mesh.TriangleIndices.Add(d);
                mesh.TriangleIndices.Add(f);
                mesh.TriangleIndices.Add(e);

                //mesh.TriangleIndices.Add(b);
                //mesh.TriangleIndices.Add(e);
                //mesh.TriangleIndices.Add(c);
                //mesh.TriangleIndices.Add(e);
                //mesh.TriangleIndices.Add(f);
                //mesh.TriangleIndices.Add(c);
            }
        }
    }
    class Polygon : Shape
    {
        private Point3D str;
        private Vector3D dir;
        private SectionPoly poly;

        public Polygon(Point3D str, Vector3D dir, SectionPoly poly)
        {
            this.str = str;
            this.dir = dir;
            this.poly = poly;

            mesh = MeshGenerator.Polygon(poly, dir.Length);
            Vector3D up = new Vector3D(1, 0, 0);
            SetTransforms(str, dir);
        }
    }
    class Arrow : Shape
    {
        private Point3D str;
        private Vector3D dir;
        private double dia;
        private int resolution;
        private bool closed;

        public Arrow(Point3D str, Vector3D dir, double dia, int resolution)
        {
            this.str = str;
            this.dir = dir;
            this.dia = dia;
            this.resolution = resolution;
            closed = false;

            double diaHead = dia * 1.7;
            double diaBody = dia;
            double heightHead = dir.Length * 0.25;
            double heightBody = dir.Length - heightHead;

            mesh = MeshGenerator.Arrow(diaHead, diaBody, heightHead, heightBody, resolution);
            Vector3D up = new Vector3D(0, 0, 1);
            SetTransforms(str, dir);
        }
    }
    class Sphere : Shape
    {
        private Point3D center;
        private double diameter;
        private int resolution;

        public Sphere(Point3D center, double diameter, int resolution)
        {
            this.center = center;
            this.diameter = diameter;
            this.resolution = resolution;

            mesh = MeshGenerator.Sphere(diameter, resolution);
            SetTransforms(center, new Vector3D(1, 1, 1));
        }
    }
    public class TextShapes : List<Text>
    {
        public Text Add(string caption, Point3D position, double size)
        {
            Text t = new Text(caption, position, size, Colors.Black);
            base.Add(t);
            return t;
        }
        internal Model3DGroup Model3DGroup()
        {
            Model3DGroup model3DGroup = new Model3DGroup();
            foreach (Text text in this)
            {
                model3DGroup.Children.Add(text.GeoModel());
            }
            //model3DGroup.Transform = transform;
            return model3DGroup;
        }

        internal ModelVisual3D ModelVisual3D()
        {
            ModelVisual3D modelVisual3D = new ModelVisual3D();
            modelVisual3D.Content = Model3DGroup();
            return modelVisual3D;
        }
    }
    public class Text : Shape
    {
        string caption;
        Point3D position;
        double size = 8;
        Color color;

        public Text(string caption, Point3D position, double size, Color color)
        {
            this.caption = caption;
            this.position = position;
            this.size = size;
            this.color = color;
        }

        internal new GeometryModel3D GeoModel()
        {
            GeometryModel3D geo3D;
            geo3D = DRAW.CreateTextLabel3D(caption, Brushes.Red, true, 1, position,
                new Vector3D(0, 0.2, 0), new Vector3D(0, 0, 0.5));
            return geo3D;
        }
    }
    static class MeshGenerator
    {
        internal static MeshGeometry3D Circle(double radius, int resolution)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.Positions.Add(new Point3D(0, 0, 0));

            // Iterate from angle 0 to 2*PI
            double t = 2 * Math.PI / resolution;
            for (int i = 0; i < resolution; i++)
            {
                mesh.Positions.Add(new Point3D(radius * Math.Cos(t * i), 0, -radius * Math.Sin(t * i)));
            }

            // Add points to MeshGeoemtry3D
            for (int i = 0; i < resolution; i++)
            {
                var a = 0;
                var b = i + 1;
                var c = (i < (resolution - 1)) ? i + 2 : 1;

                mesh.TriangleIndices.Add(a);
                mesh.TriangleIndices.Add(b);
                mesh.TriangleIndices.Add(c);
            }
            return mesh;
        }
        internal static MeshGeometry3D Cone(double radius, double height, int resolution)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.Positions.Add(new Point3D(0, 0, height));

            // Iterate from angle 0 to 2*PI
            double t = 2 * Math.PI / resolution;
            for (int i = 0; i < resolution; i++)
            {
                mesh.Positions.Add(new Point3D(radius * Math.Cos(t * i), -radius * Math.Sin(t * i), 0));
            }

            // Add points to MeshGeoemtry3D
            for (int i = 0; i < resolution; i++)
            {
                var a = 0;
                var b = i + 1;
                var c = (i < (resolution - 1)) ? i + 2 : 1;

                mesh.TriangleIndices.Add(a);
                mesh.TriangleIndices.Add(c);
                mesh.TriangleIndices.Add(b);
            }
            return mesh;
        }
        internal static MeshGeometry3D Cylinder(double dia, double length, int resolution)
        {
            return Cylinder(dia, length, resolution, 0);
        }
        internal static MeshGeometry3D Cylinder(double dia, double length, int resolution, double rotationAngle)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.Positions.Add(new Point3D(0, 0, 0));

            // Iterate from angle 0 to 2*PI
            double t = 2 * Math.PI / resolution;
            double radius = dia / 2;
            double height = length;
            for (int i = 0; i < resolution; i++)
            {
                mesh.Positions.Add(new Point3D(radius * Math.Cos(t * i + rotationAngle), -radius * Math.Sin(t * i + rotationAngle), 0));
            }

            mesh.Positions.Add(new Point3D(0, 0, height));
            for (int i = 0; i < resolution; i++)
            {
                mesh.Positions.Add(new Point3D(radius * Math.Cos(t * i + rotationAngle), -radius * Math.Sin(t * i + rotationAngle), height));
            }

            for (int i = 0; i < resolution; i++)
            {
                //int a = 0;
                int b = i + 1;
                int c = (i < (resolution - 1)) ? i + 2 : 1;
                //int d = a + resolution + 1;
                int e = b + resolution + 1;
                int f = c + resolution + 1;

                //mesh.TriangleIndices.Add(a);
                //mesh.TriangleIndices.Add(b);
                //mesh.TriangleIndices.Add(c);
                //mesh.TriangleIndices.Add(d);
                //mesh.TriangleIndices.Add(f);
                //mesh.TriangleIndices.Add(e);

                mesh.TriangleIndices.Add(b);
                mesh.TriangleIndices.Add(e);
                mesh.TriangleIndices.Add(c);
                mesh.TriangleIndices.Add(e);
                mesh.TriangleIndices.Add(f);
                mesh.TriangleIndices.Add(c);
            }
            return mesh;
        }
        internal static MeshGeometry3D Arrow(double diaHead, double diaBody, double heightHead, double heightBody, int resolution)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            double t = 2 * Math.PI / resolution;
            double radiusHead = diaHead / 2;
            double radiusBody = diaBody / 2;

            //화살표 body 하부 원의 절점 추가
            mesh.Positions.Add(new Point3D(0, 0, 0));
            for (int i = 0; i < resolution; i++)
            {
                mesh.Positions.Add(new Point3D(radiusBody * Math.Cos(t * i), -radiusBody * Math.Sin(t * i), 0));
            }

            //화살표 body 상부 원의 절점 추가
            mesh.Positions.Add(new Point3D(0, 0, heightBody));
            for (int i = 0; i < resolution; i++)
            {
                mesh.Positions.Add(new Point3D(radiusBody * Math.Cos(t * i), -radiusBody * Math.Sin(t * i), heightBody));
            }

            //화살표 머리 하부 원의 절점 추가
            mesh.Positions.Add(new Point3D(0, 0, heightBody));
            for (int i = 0; i < resolution; i++)
            {
                mesh.Positions.Add(new Point3D(radiusHead * Math.Cos(t * i), -radiusHead * Math.Sin(t * i), heightBody));
            }

            //화살표 꼭지점 추가
            mesh.Positions.Add(new Point3D(0, 0, heightBody + heightHead));

            //화살표 body 둥근 껍데기
            for (int i = 0; i < resolution; i++)
            {
                //바닥 절점들
                int a = 0;
                int b = i + 1;
                int c = (i < (resolution - 1)) ? i + 2 : 1;
                //Body 상부 절점들
                int d = a + resolution + 1; //Body 상부 센터 절점
                int e = b + resolution + 1;
                int f = c + resolution + 1;
                //Head 바닥 절점들
                int g = resolution * 3 + 3; //화살표 꼭지점
                int h = e + resolution + 1;
                int ii = f + resolution + 1;

                //바닥
                mesh.TriangleIndices.Add(a);
                mesh.TriangleIndices.Add(b);
                mesh.TriangleIndices.Add(c);

                //Body
                mesh.TriangleIndices.Add(b);
                mesh.TriangleIndices.Add(e);
                mesh.TriangleIndices.Add(c);
                mesh.TriangleIndices.Add(e);
                mesh.TriangleIndices.Add(f);
                mesh.TriangleIndices.Add(c);

                //목덜미 - 그냥 닫아버리는게 개수가 적게 들어가므로 그냥 닫아버림. 사각형으로 하면 시간이 더 많이 걸릴듯.
                mesh.TriangleIndices.Add(d);
                mesh.TriangleIndices.Add(h);
                mesh.TriangleIndices.Add(ii);

                //콘
                mesh.TriangleIndices.Add(g);
                mesh.TriangleIndices.Add(ii);
                mesh.TriangleIndices.Add(h);
            }

            return mesh;
        }
        internal static MeshGeometry3D Hexahedron(Point3D p0, Point3D p1, Point3D p2, Point3D p3, Point3D p4, Point3D p5, Point3D p6, Point3D p7)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.Positions.Add(p0);
            mesh.Positions.Add(p1);
            mesh.Positions.Add(p2);
            mesh.Positions.Add(p3);
            mesh.Positions.Add(p4);
            mesh.Positions.Add(p5);
            mesh.Positions.Add(p6);
            mesh.Positions.Add(p7);

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(0);

            mesh.TriangleIndices.Add(4);
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(4);

            mesh.TriangleIndices.Add(7);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(4);
            mesh.TriangleIndices.Add(7);

            mesh.TriangleIndices.Add(7);
            mesh.TriangleIndices.Add(4);
            mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(6);
            mesh.TriangleIndices.Add(7);

            mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(6);
            mesh.TriangleIndices.Add(5);

            mesh.TriangleIndices.Add(6);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(7);
            mesh.TriangleIndices.Add(6);
            return mesh;
        }
        internal static MeshGeometry3D Line3D(double length)
        {
            double dia = 0.1;
            int resolution = 3;

            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.Positions.Add(new Point3D(0, 0, 0));

            // Iterate from angle 0 to 2*PI
            double t = 2 * Math.PI / resolution;
            double radius = dia / 2;
            double height = length;
            for (int i = 0; i < resolution; i++)
            {
                mesh.Positions.Add(new Point3D(radius * Math.Cos(t * i), -radius * Math.Sin(t * i), 0));
            }

            mesh.Positions.Add(new Point3D(0, 0, height));
            for (int i = 0; i < resolution; i++)
            {
                mesh.Positions.Add(new Point3D(radius * Math.Cos(t * i), -radius * Math.Sin(t * i), height));
            }

            for (int i = 0; i < resolution; i++)
            {
                //int a = 0;
                int b = i + 1;
                int c = (i < (resolution - 1)) ? i + 2 : 1;
                //int d = a + resolution + 1;
                int e = b + resolution + 1;
                int f = c + resolution + 1;

                //mesh.TriangleIndices.Add(a);
                //mesh.TriangleIndices.Add(b);
                //mesh.TriangleIndices.Add(c);
                //mesh.TriangleIndices.Add(d);
                //mesh.TriangleIndices.Add(f);
                //mesh.TriangleIndices.Add(e);

                mesh.TriangleIndices.Add(b);
                mesh.TriangleIndices.Add(e);
                mesh.TriangleIndices.Add(c);
                mesh.TriangleIndices.Add(e);
                mesh.TriangleIndices.Add(f);
                mesh.TriangleIndices.Add(c);
            }
            return mesh;
        }
        internal static MeshGeometry3D Sphere(double diameter, int resolution)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            int numLongitudePoint = resolution;
            int numLatitudePoint = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(resolution) / 2 + 1)) - 1;

            double longitude;
            double latitude;

            mesh.Positions.Add(new Point3D(0, 0, -diameter / 2));
            for (int j = 0; j < numLatitudePoint; j++)
            {
                latitude = Math.PI / numLatitudePoint * (j + 1) - Math.PI / 2;
                double r = diameter / 2 * Math.Cos(latitude);
                double z = diameter / 2 * Math.Sin(latitude);

                for (int i = 0; i < numLongitudePoint; i++)
                {
                    longitude = Math.PI * 2 / numLongitudePoint * i;
                    double x = r * Math.Cos(longitude);
                    double y = r * Math.Sin(longitude);
                    mesh.Positions.Add(new Point3D(x, y, z));
                }
            }
            mesh.Positions.Add(new Point3D(0, 0, diameter / 2));

            int buttonPointNumber = 0;
            int topPointNumber = (numLatitudePoint - 1) * numLongitudePoint + 1;

            //Bottom Cap
            for (int i = 0; i < numLongitudePoint - 1; i++)
            {
                mesh.TriangleIndices.Add(buttonPointNumber);
                mesh.TriangleIndices.Add(i + 2);
                mesh.TriangleIndices.Add(i + 1);
            }
            mesh.TriangleIndices.Add(buttonPointNumber);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(numLongitudePoint);

            //Top Cap
            for (int i = 0; i < numLongitudePoint - 1; i++)
            {
                mesh.TriangleIndices.Add(topPointNumber);
                mesh.TriangleIndices.Add(topPointNumber - i - 2);
                mesh.TriangleIndices.Add(topPointNumber - i - 1);
            }
            mesh.TriangleIndices.Add(topPointNumber);
            mesh.TriangleIndices.Add(topPointNumber - 1);
            mesh.TriangleIndices.Add(topPointNumber - numLongitudePoint);

            int p0;
            int p1;
            int p2;
            int p3;

            for (int j = 0; j < numLatitudePoint - 1; j++)
            {
                for (int i = 0; i < numLongitudePoint - 1; i++)
                {
                    p0 = i + 1 + numLongitudePoint * j;
                    p1 = i + 2 + numLongitudePoint * j;
                    p2 = p1 + numLongitudePoint;
                    p3 = p0 + numLongitudePoint;

                    mesh.TriangleIndices.Add(p0);
                    mesh.TriangleIndices.Add(p1);
                    mesh.TriangleIndices.Add(p2);

                    mesh.TriangleIndices.Add(p2);
                    mesh.TriangleIndices.Add(p3);
                    mesh.TriangleIndices.Add(p0);
                }
                p0 = numLongitudePoint + numLongitudePoint * j;
                p1 = 1 + numLongitudePoint * j;
                p2 = p1 + numLongitudePoint;
                p3 = p0 + numLongitudePoint;

                mesh.TriangleIndices.Add(p0);
                mesh.TriangleIndices.Add(p1);
                mesh.TriangleIndices.Add(p2);

                mesh.TriangleIndices.Add(p2);
                mesh.TriangleIndices.Add(p3);
                mesh.TriangleIndices.Add(p0);
            }
            return mesh;
        }
        internal static MeshGeometry3D Triangle(Point3D p0, Point3D p1, Point3D p2)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.Positions.Add(p0);
            mesh.Positions.Add(p1);
            mesh.Positions.Add(p2);

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(1);

            Vector3D normal = new Vector3D(0, 0, 1);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
            return mesh;
        }
        internal static MeshGeometry3D Polygon(SectionPoly poly, double length)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            mesh.Positions.Add(new Point3D(0, 0, 0));
            double height = length;
            foreach (SectionPolyPoint spp in poly)
            {
                mesh.Positions.Add(new Point3D(spp.X, spp.Y, 0));
            }
            mesh.Positions.Add(new Point3D(0, 0, height));
            foreach (SectionPolyPoint spp in poly)
            {
                mesh.Positions.Add(new Point3D(spp.X, spp.Y, height));
            }

            for (int i = 0; i < poly.Count; i++)
            {
                //int a = 0;
                int b = i + 1;
                int c = (i < (poly.Count - 1)) ? i + 2 : 1;
                //int d = a + resolution + 1;
                int e = b + poly.Count + 1;
                int f = c + poly.Count + 1;

                //mesh.TriangleIndices.Add(a);
                //mesh.TriangleIndices.Add(b);
                //mesh.TriangleIndices.Add(c);
                //mesh.TriangleIndices.Add(d);
                //mesh.TriangleIndices.Add(f);
                //mesh.TriangleIndices.Add(e);

                mesh.TriangleIndices.Add(b);
                mesh.TriangleIndices.Add(c);
                mesh.TriangleIndices.Add(e);
                mesh.TriangleIndices.Add(e);
                mesh.TriangleIndices.Add(c);
                mesh.TriangleIndices.Add(f);
            }
            return mesh;
        }
    }

}
