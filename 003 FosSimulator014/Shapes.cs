using System.Collections.Generic;
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
    }

}
