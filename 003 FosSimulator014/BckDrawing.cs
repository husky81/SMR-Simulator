using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace _003_FosSimulator014
{
    class BckDrawing001
    {
        private Grid grid;
        private readonly int iniLineThickness = 2;
        private Model3DGroup modelGroup = new Model3DGroup();
        Viewport3D myViewport3D = new Viewport3D();

        private int viewDistance = 100;

        private PerspectiveCamera iniPCamera = new PerspectiveCamera();
        private PerspectiveCamera myPCamera = new PerspectiveCamera
        {
            Position = new Point3D(11, 10, 9),
            LookDirection = new Vector3D(-11, -10, -9),
            FieldOfView = 70,
            FarPlaneDistance = 200,
            UpDirection = new Vector3D(0, 1, 0),
            NearPlaneDistance = 1
        };

        DirectionalLight myDirLight = new DirectionalLight
        {
            Color = Colors.White,
            Direction = new Vector3D(-3, -4, -5)
        };

        public BckDrawing001(Grid grid)
        {
            //생성자
            this.Grid = grid;

        }
        public Grid Grid { get => grid; set => grid = value; }
        public PerspectiveCamera MyPCamera { get => myPCamera; set => myPCamera = value; }
        public Model3DGroup ModelGroup { get => modelGroup; set => modelGroup = value; }

        public void DrawSampleGradient()
        {
            // Declare scene objects.
            Viewport3D myViewport3D = new Viewport3D();
            Model3DGroup myModel3DGroup = new Model3DGroup();
            GeometryModel3D myGeometryModel = new GeometryModel3D();
            ModelVisual3D myModelVisual3D = new ModelVisual3D();
            // Defines the camera used to view the 3D object. In order to view the 3D object,
            // the camera must be positioned and pointed such that the object is within view 
            // of the camera.
            PerspectiveCamera myPCamera = new PerspectiveCamera
            {
                // Specify where in the 3D scene the camera is.
                Position = new Point3D(0, 0, 2),

                // Specify the direction that the camera is pointing.
                LookDirection = new Vector3D(0, 0, -1),

                // Define camera's horizontal field of view in degrees.
                FieldOfView = 60
            };

            // Asign the camera to the viewport
            myViewport3D.Camera = myPCamera;
            // Define the lights cast in the scene. Without light, the 3D object cannot 
            // be seen. Note: to illuminate an object from additional directions, create 
            // additional lights.
            DirectionalLight myDirectionalLight = new DirectionalLight
            {
                Color = Colors.White,
                Direction = new Vector3D(-0.61, -0.5, -0.61)
            };

            myModel3DGroup.Children.Add(myDirectionalLight);

            // The geometry specifes the shape of the 3D plane. In this sample, a flat sheet 
            // is created.
            MeshGeometry3D myMeshGeometry3D = new MeshGeometry3D();

            // Create a collection of normal vectors for the MeshGeometry3D.
            Vector3DCollection myNormalCollection = new Vector3DCollection
            {
                new Vector3D(0, 0, 1),
                new Vector3D(0, 0, 1),
                new Vector3D(0, 0, 1),
                new Vector3D(0, 0, 1),
                new Vector3D(0, 0, 1),
                new Vector3D(0, 0, 1)
            };
            myMeshGeometry3D.Normals = myNormalCollection;

            // Create a collection of vertex positions for the MeshGeometry3D. 
            Point3DCollection myPositionCollection = new Point3DCollection
            {
                new Point3D(-0.5, -0.5, 0.5),
                new Point3D(0.5, -0.5, 0.5),
                new Point3D(0.5, 0.5, 0.5),
                new Point3D(0.5, 0.5, 0.5),
                new Point3D(-0.5, 0.5, 0.5),
                new Point3D(-0.5, -0.5, 0.5)
            };
            myMeshGeometry3D.Positions = myPositionCollection;

            // Create a collection of texture coordinates for the MeshGeometry3D.
            PointCollection myTextureCoordinatesCollection = new PointCollection
            {
                new Point(0, 0),
                new Point(1, 0),
                new Point(1, 1),
                new Point(1, 1),
                new Point(0, 1),
                new Point(0, 0)
            };
            myMeshGeometry3D.TextureCoordinates = myTextureCoordinatesCollection;

            // Create a collection of triangle indices for the MeshGeometry3D.
            Int32Collection myTriangleIndicesCollection = new Int32Collection
            {
                0,
                1,
                2,
                3,
                4,
                5
            };
            myMeshGeometry3D.TriangleIndices = myTriangleIndicesCollection;

            // Apply the mesh to the geometry model.
            myGeometryModel.Geometry = myMeshGeometry3D;

            // The material specifies the material applied to the 3D object. In this sample a  
            // linear gradient covers the surface of the 3D object.

            // Create a horizontal linear gradient with four stops.   
            LinearGradientBrush myHorizontalGradient = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0.5),
                EndPoint = new Point(1, 0.5)
            };
            myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.Yellow, 0.0));
            myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.Red, 0.25));
            myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.Blue, 0.75));
            myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.LimeGreen, 1.0));

            // Define material and apply to the mesh geometries.
            DiffuseMaterial myMaterial = new DiffuseMaterial(myHorizontalGradient);
            myGeometryModel.Material = myMaterial;

            // Apply a transform to the object. In this sample, a rotation transform is applied,  
            // rendering the 3D object rotated.
            RotateTransform3D myRotateTransform3D = new RotateTransform3D();
            AxisAngleRotation3D myAxisAngleRotation3d = new AxisAngleRotation3D
            {
                Axis = new Vector3D(0, 3, 0),
                Angle = 40
            };
            myRotateTransform3D.Rotation = myAxisAngleRotation3d;
            myGeometryModel.Transform = myRotateTransform3D;

            // Add the geometry model to the model group.
            myModel3DGroup.Children.Add(myGeometryModel);

            // Add the group of models to the ModelVisual3d.
            myModelVisual3D.Content = myModel3DGroup;

            // 
            myViewport3D.Children.Add(myModelVisual3D);

            // Apply the viewport to the page so it will be rendered.
            //this.Content = myViewport3D;
            grid.Children.Add(myViewport3D);

            //grdBackground.Visibility = Visibility.Hidden;

        }


        internal void ViewZoomExtend()
        {
            
        }

        internal void ViewBack()
        {
            PerspectiveCamera newCamera = new PerspectiveCamera
            {
                Position = new Point3D(0, -viewDistance, 0),
                LookDirection = new Vector3D(0, viewDistance, 0),
                UpDirection = new Vector3D(0, 0, 1)
            };

            myPCamera.Position = newCamera.Position;
            myPCamera.LookDirection = newCamera.LookDirection;
            myPCamera.UpDirection = newCamera.UpDirection;
        }

        internal void ViewRight()
        {
            PerspectiveCamera newCamera = new PerspectiveCamera
            {
                Position = new Point3D(0, 0, viewDistance),
                LookDirection = new Vector3D(0, 0, -viewDistance),
                UpDirection = new Vector3D(0, 1, 0)
            };

            myPCamera.Position = newCamera.Position;
            myPCamera.LookDirection = newCamera.LookDirection;
            myPCamera.UpDirection = newCamera.UpDirection;
        }

        internal void ViewLeft()
        {
            PerspectiveCamera newCamera = new PerspectiveCamera
            {
                Position = new Point3D(0, -viewDistance, 0),
                LookDirection = new Vector3D(0, +viewDistance, 0),
                UpDirection = new Vector3D(0, 0, 1)
            };

            myPCamera.Position = newCamera.Position;
            myPCamera.LookDirection = newCamera.LookDirection;
            myPCamera.UpDirection = newCamera.UpDirection;
        }

        internal void ViewBottom()
        {
            PerspectiveCamera newCamera = new PerspectiveCamera
            {
                Position = new Point3D(0, 0, viewDistance),
                LookDirection = new Vector3D(0, 0, -viewDistance),
                UpDirection = new Vector3D(0, 1, 0)
            };

            myPCamera.Position = newCamera.Position;
            myPCamera.LookDirection = newCamera.LookDirection;
            myPCamera.UpDirection = newCamera.UpDirection;
        }

        internal void ViewFront()
        {
            PerspectiveCamera newCamera = new PerspectiveCamera
            {
                Position = new Point3D(0, 0, viewDistance),
                LookDirection = new Vector3D(0, 0, -viewDistance),
                UpDirection = new Vector3D(0, 1, 0)
            };

            myPCamera.Position = newCamera.Position;
            myPCamera.LookDirection = newCamera.LookDirection;
            myPCamera.UpDirection = newCamera.UpDirection;
        }

        internal void ViewTop()
        {
            PerspectiveCamera newCamera = new PerspectiveCamera
            {
                Position = new Point3D(0, 0, viewDistance),
                LookDirection = new Vector3D(0, 0, -viewDistance),
                UpDirection = new Vector3D(0, 1, 0)
            };

            myPCamera.Position = newCamera.Position;
            myPCamera.LookDirection = newCamera.LookDirection;
            myPCamera.UpDirection = newCamera.UpDirection;
            //MotionMoveCamera(newCamera);
        }

        private void MotionMoveCamera(PerspectiveCamera newCamera)
        {
            int numFrame = 100;
            int dwStartTime = System.Environment.TickCount;

            Point3D sp = myPCamera.Position;
            Point3D ep = newCamera.Position;
            Vector3D dp = new Vector3D(ep.X - sp.X, ep.Y - sp.Y, ep.Z - sp.Z);
            dp /= numFrame;

            Vector3D sl = myPCamera.LookDirection;
            Vector3D el = newCamera.LookDirection;
            Vector3D dl = new Vector3D(el.X - sl.X, el.Y - sl.Y, el.Z - sl.Z);
            dl /= numFrame;

            Vector3D su = myPCamera.UpDirection;
            Vector3D eu = newCamera.UpDirection;
            Vector3D du = new Vector3D(eu.X - su.X, eu.Y - su.Y, eu.Z - su.Z);
            du /= numFrame;


            for (int i = 0; i <= numFrame; i++)
            {
                myPCamera.Position = new Point3D(sp.X + dp.X * i, sp.Y + dp.Y * i, sp.Z + dp.Z * i);
                myPCamera.LookDirection = new Vector3D(sl.X + dl.X * i, sl.Y + dl.Y * i, sl.Z + dl.Z * i);
                myPCamera.UpDirection = new Vector3D(su.X + du.X * i, su.Y + du.Y * i, su.Z + du.Z * i);

                while (true)
                {
                    if (System.Environment.TickCount - dwStartTime > 1000) break; //1000 milliseconds 
                }
                myViewport3D.UpdateLayout();
                
            }

            //myPCamera.Position = ep;
            //myPCamera.LookDirection = el;
            //myPCamera.UpDirection = eu;

        }

        internal void DrawCube(Point3D c, Vector3D b)
        {
            Point3D p0 = new Point3D(c.X, c.Y, c.Z);
            Point3D p1 = new Point3D(c.X + b.X, c.Y, c.Z);
            Point3D p2 = new Point3D(c.X + b.X, c.Y + b.Y, c.Z);
            Point3D p3 = new Point3D(c.X, c.Y + b.Y, c.Z);
            Point3D p4 = new Point3D(c.X, c.Y, c.Z+b.Z);
            Point3D p5 = new Point3D(c.X + b.X, c.Y, c.Z + b.Z);
            Point3D p6 = new Point3D(c.X + b.X, c.Y + b.Y, c.Z + b.Z);
            Point3D p7 = new Point3D(c.X, c.Y + b.Y, c.Z + b.Z);

        }
        internal void DrawHexahedron(Point3D p0, Point3D p1, Point3D p2, Point3D p3, Point3D p4, Point3D p5, Point3D p6, Point3D p7)
        {
            DrawHexahedron(p0, p1, p2, p3, p4, p5, p6, p7, Colors.Blue);
        }
        internal void DrawHexahedron(Point3D p0, Point3D p1, Point3D p2, Point3D p3, Point3D p4, Point3D p5, Point3D p6, Point3D p7,Color color)
        {
            DrawRectangle(p0, p1, p2, p3, color);
            DrawRectangle(p4, p5, p1, p0, color);
            DrawRectangle(p7, p4, p0, p3, color);
            DrawRectangle(p7, p6, p5, p4, color);
            DrawRectangle(p5, p6, p2, p1, color);
            DrawRectangle(p6, p7, p3, p2, color);

            //DrawRectangle(p0, p1, p2, p3, Colors.Black);
            //DrawRectangle(p4, p5, p1, p0, Colors.Red);
            //DrawRectangle(p7, p4, p0, p3, Colors.Cyan);
            //DrawRectangle(p7, p6, p5, p4, Colors.Gray);
            //DrawRectangle(p5, p6, p2, p1, Colors.Yellow);
            //DrawRectangle(p6, p7, p3, p2, Colors.Gold);

            //DrawTriangle(p0, p1, p2);
            //DrawTriangle(p2, p3, p0);
            //DrawTriangle(p0, p1, p4);
            //DrawTriangle(p1, p5, p4);
            //DrawTriangle(p1, p2, p5);
            //DrawTriangle(p2, p6, p5);
            //DrawTriangle(p2, p3, p6);
            //DrawTriangle(p3, p7, p6);
            //DrawTriangle(p3, p0, p7);
            //DrawTriangle(p0, p4, p7);
            //DrawTriangle(p4, p5, p6);
            //DrawTriangle(p6, p7, p4);
        }
        internal void DrawRectangle(Point3D p0, Point3D p1, Point3D p2, Point3D p3)
        {
            DrawRectangle(p0, p1, p2, p3, Colors.Blue);
        }

        internal void DrawRectangle(Point3D p0, Point3D p1, Point3D p2, Point3D p3,Color color)
        {
            DrawTriangle(p0, p1, p2, color);
            DrawTriangle(p2, p3, p0, color);
        }

        internal void DrawSampleTriangle()
        {

            MeshGeometry3D triangleMesh = new MeshGeometry3D();
            Point3D point0 = new Point3D(0, 0, 0);
            Point3D point1 = new Point3D(5, 0, 0);
            Point3D point2 = new Point3D(0, 0, 5);

            triangleMesh.Positions.Add(point0);
            triangleMesh.Positions.Add(point1);
            triangleMesh.Positions.Add(point2);

            triangleMesh.TriangleIndices.Add(0);
            triangleMesh.TriangleIndices.Add(2);
            triangleMesh.TriangleIndices.Add(1);

            Vector3D normal = new Vector3D(0, 1, 0);
            triangleMesh.Normals.Add(normal);
            triangleMesh.Normals.Add(normal);
            triangleMesh.Normals.Add(normal);

            Material material = new DiffuseMaterial(new SolidColorBrush(Colors.DarkKhaki));
            GeometryModel3D triangleModel = new GeometryModel3D(triangleMesh, material);

            Model3DGroup modelGroup = new Model3DGroup();
            modelGroup.Children.Add(triangleModel);


            Viewport3D myViewport3D = new Viewport3D();
            // Asign the camera to the viewport
            myViewport3D.Camera = MyPCamera;

            DirectionalLight myDirLight = new DirectionalLight
            {
                Color = Colors.White,
                Direction = new Vector3D(-3, -4, -5)
            };

            modelGroup.Children.Add(myDirLight);

            ModelVisual3D model = new ModelVisual3D
            {
                Content = modelGroup
            };


            myViewport3D.Children.Add(model);

            // Apply the viewport to the page so it will be rendered.
            //this.Content = myViewport3D;
            grid.Children.Add(myViewport3D);

            //myPCamera.FarPlaneDistance = 15;

        }
        internal void DrawTriangle(Point3D p0, Point3D p1, Point3D p2)
        {
            DrawTriangle(p0, p1, p2, Colors.DarkKhaki);
        }

        internal void DrawTriangle(Point3D p0, Point3D p1, Point3D p2, Color color)
        {
            MeshGeometry3D triangleMesh = new MeshGeometry3D();
            triangleMesh.Positions.Add(p0);
            triangleMesh.Positions.Add(p1);
            triangleMesh.Positions.Add(p2);

            triangleMesh.TriangleIndices.Add(0);
            triangleMesh.TriangleIndices.Add(2);
            triangleMesh.TriangleIndices.Add(1);

            Vector3D normal = new Vector3D(0, 1, 0);
            triangleMesh.Normals.Add(normal);
            triangleMesh.Normals.Add(normal);
            triangleMesh.Normals.Add(normal);

            Material material = new DiffuseMaterial(new SolidColorBrush(color));
            GeometryModel3D triangleModel = new GeometryModel3D(triangleMesh, material);

            ModelGroup.Children.Add(triangleModel);
            ModelGroup.Children.Add(myDirLight);

            ModelVisual3D model = new ModelVisual3D
            {
                Content = ModelGroup
            };
            myViewport3D.Children.Clear();
            myViewport3D.Children.Add(model);
            myViewport3D.Camera = myPCamera;

            grid.Children.Clear();

            grid.Children.Add(myViewport3D);

        }

        /// <summary>
        /// Generates a model of a Circle given specified parameters
        /// </summary>
        /// <param name="radius">Radius of circle</param>
        /// <param name="normal">Vector normal to circle's plane</param>
        /// <param name="center">Center position of the circle</param>
        /// <param name="resolution">Number of slices to iterate the circumference of the circle</param>
        /// <returns>A GeometryModel3D representation of the circle</returns>
        private GeometryModel3D GetCircleModel(double radius, Vector3D normal, Point3D center, int resolution)
        {
            var mod = new GeometryModel3D();
            var geo = new MeshGeometry3D();

            // Generate the circle in the XZ-plane
            // Add the center first
            geo.Positions.Add(new Point3D(0, 0, 0));

            // Iterate from angle 0 to 2*PI
            double t = 2 * Math.PI / resolution;
            for (int i = 0; i < resolution; i++)
            {
                geo.Positions.Add(new Point3D(radius * Math.Cos(t * i), 0, -radius * Math.Sin(t * i)));
            }

            // Add points to MeshGeoemtry3D
            for (int i = 0; i < resolution; i++)
            {
                var a = 0;
                var b = i + 1;
                var c = (i < (resolution - 1)) ? i + 2 : 1;

                geo.TriangleIndices.Add(a);
                geo.TriangleIndices.Add(b);
                geo.TriangleIndices.Add(c);
            }

            mod.Geometry = geo;

            // Create transforms
            var trn = new Transform3DGroup();
            // Up Vector (normal for XZ-plane)
            var up = new Vector3D(0, 1, 0);
            // Set normal length to 1
            normal.Normalize();
            var axis = Vector3D.CrossProduct(up, normal); // Cross product is rotation axis
            var angle = Vector3D.AngleBetween(up, normal); // Angle to rotate
            trn.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(axis, angle)));
            trn.Children.Add(new TranslateTransform3D(new Vector3D(center.X, center.Y, center.Z)));

            mod.Transform = trn;

            return mod;
        }
        internal void DrawCircle(double radius, Vector3D normal, Point3D center, int resolution,Color color)
        {
            var geo = new MeshGeometry3D();

            // Generate the circle in the XZ-plane
            // Add the center first
            geo.Positions.Add(new Point3D(0, 0, 0));

            // Iterate from angle 0 to 2*PI
            double t = 2 * Math.PI / resolution;
            for (int i = 0; i < resolution; i++)
            {
                geo.Positions.Add(new Point3D(radius * Math.Cos(t * i), 0, -radius * Math.Sin(t * i)));
            }

            // Add points to MeshGeoemtry3D
            for (int i = 0; i < resolution; i++)
            {
                var a = 0;
                var b = i + 1;
                var c = (i < (resolution - 1)) ? i + 2 : 1;

                geo.TriangleIndices.Add(a);
                geo.TriangleIndices.Add(b);
                geo.TriangleIndices.Add(c);
            }

            Material material = new DiffuseMaterial(new SolidColorBrush(color));
            GeometryModel3D mod = new GeometryModel3D(geo, material);


            // Create transforms
            var trn = new Transform3DGroup();
            // Up Vector (normal for XZ-plane)
            var up = new Vector3D(0, 1, 0);
            // Set normal length to 1
            normal.Normalize();
            var axis = Vector3D.CrossProduct(up, normal); // Cross product is rotation axis
            var angle = Vector3D.AngleBetween(up, normal); // Angle to rotate
            trn.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(axis, angle)));
            trn.Children.Add(new TranslateTransform3D(new Vector3D(center.X, center.Y, center.Z)));
            
            mod.Transform = trn;


            ModelGroup.Children.Add(mod);
            ModelGroup.Children.Add(myDirLight);

            ModelVisual3D model = new ModelVisual3D
            {
                Content = ModelGroup
            };
            myViewport3D.Children.Clear();
            myViewport3D.Children.Add(model);
            myViewport3D.Camera = myPCamera;

            grid.Children.Clear();

            grid.Children.Add(myViewport3D);

        }

        internal void DrawCone(Point3D center, double radius, Vector3D heightVector, int resolution, Color color)
        {
            var geo = new MeshGeometry3D();

            double height = heightVector.Length;

            // Generate the circle in the XZ-plane
            // Add the center first
            geo.Positions.Add(new Point3D(0, height, 0));

            // Iterate from angle 0 to 2*PI
            double t = 2 * Math.PI / resolution;
            for (int i = 0; i < resolution; i++)
            {
                geo.Positions.Add(new Point3D(radius * Math.Cos(t * i), 0, -radius * Math.Sin(t * i)));
            }

            // Add points to MeshGeoemtry3D
            for (int i = 0; i < resolution; i++)
            {
                var a = 0;
                var b = i + 1;
                var c = (i < (resolution - 1)) ? i + 2 : 1;

                geo.TriangleIndices.Add(a);
                geo.TriangleIndices.Add(b);
                geo.TriangleIndices.Add(c);
            }

            Material material = new DiffuseMaterial(new SolidColorBrush(color));
            GeometryModel3D mod = new GeometryModel3D(geo, material);


            // Create transforms
            var trn = new Transform3DGroup();
            // Up Vector (normal for XZ-plane)
            var up = new Vector3D(0, 1, 0);
            // Set normal length to 1
            heightVector.Normalize();
            var axis = Vector3D.CrossProduct(up, heightVector); // Cross product is rotation axis
            var angle = Vector3D.AngleBetween(up, heightVector); // Angle to rotate
            trn.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(axis, angle)));
            trn.Children.Add(new TranslateTransform3D(new Vector3D(center.X, center.Y, center.Z)));

            mod.Transform = trn;


            ModelGroup.Children.Add(mod);
            ModelGroup.Children.Add(myDirLight);

            ModelVisual3D model = new ModelVisual3D
            {
                Content = ModelGroup
            };
            myViewport3D.Children.Clear();
            myViewport3D.Children.Add(model);
            myViewport3D.Camera = myPCamera;

            grid.Children.Clear();

            grid.Children.Add(myViewport3D);
        }


        internal void DrawLine(double x1, double x2, double y1, double y2)
        {
            DrawLine(x1, x2, y1, y2, iniLineThickness);
        }
        internal void DrawLine(double x1, double x2, double y1, double y2, double thickness)
        {
            Line myLine = new Line
            {
                Stroke = System.Windows.Media.Brushes.LightSteelBlue,
                X1 = x1,
                X2 = x2,
                Y1 = y1,
                Y2 = y2,
                //HorizontalAlignment = HorizontalAlignment.Left,
                //VerticalAlignment = VerticalAlignment.Center,
                StrokeThickness = thickness
            };
            grid.Children.Add(myLine);
        }
        internal void DrawSampleLine()
        {
            // Add a Line Element
            Line myLine = new Line
            {
                Stroke = System.Windows.Media.Brushes.LightSteelBlue,
                X1 = 1,
                X2 = 50,
                Y1 = 1,
                Y2 = 100,
                //HorizontalAlignment = HorizontalAlignment.Left,
                //VerticalAlignment = VerticalAlignment.Center,
                StrokeThickness = 5
            };
            grid.Children.Add(myLine);
            //myLine.X1 = 100;

        }
        internal void OrbitForward001(int delta)
        {
            Point3D p = myPCamera.Position;
            Vector3D pv = new Vector3D
            {
                X = p.X,
                Y = p.Y,
                Z = p.Z
            };
            Vector3D fp = pv + myPCamera.LookDirection; //초점
            Vector3D cv = pv - fp; //초점에 대한 카메라 위치 백터
            double dist = cv.Length; //초점과 카메라의 거리

            double deltaCoefForward = 1000;  //뒤로갈때와 앞으로 갈 때의 계수. 숫자가 클수록 휠 반응이 둔감해짐.
            double deltaCoefBackward = 950;

            double newDist;
            if (delta > 0)
            {
                newDist = dist * (1 - delta / deltaCoefForward);
            }
            else
            {
                newDist = dist * (1 - delta / deltaCoefBackward);
            }

            Vector3D np = fp + cv * newDist/dist;

            p.X = np.X;
            p.Y = np.Y;
            p.Z = np.Z;
            myPCamera.Position = p;
        }
        internal void OrbitForward000(int delta)
        {
            Point3D p = myPCamera.Position;
            Vector3D pv = new Vector3D
            {
                X = p.X,
                Y = p.Y,
                Z = p.Z
            };
            Vector3D fp = pv + myPCamera.LookDirection; //초점
            Vector3D cv = pv - fp; //초점에 대한 카메라 위치 백터

            Vector3D np = fp + cv * (1 - delta / 100);

            p.X = np.X;
            p.Y = np.Y;
            p.Z = np.Z;
            myPCamera.Position = p;
        }

        internal void OrbitRotate008(Vector mov)
        {
            Point3D p = iniPCamera.Position;
            Vector3D pv = new Vector3D(p.X, p.Y, p.Z);  //카메라 위치
            Vector3D u = iniPCamera.UpDirection;
            Vector3D d = iniPCamera.LookDirection;

            //초점은 가시거리의 1/5로 잡는게 좋을 것 같음.
            d.Normalize();
            double fdist = iniPCamera.FarPlaneDistance / 5;
            Vector3D fp = pv + d * fdist; //초점
            Vector3D cv = pv - fp; //초점에 대한 카메라 위치 백터

            Vector3D localXVector = Vector3D.CrossProduct(d, u);
            Vector3D localYVector = u;
            Vector3D localZVector = Vector3D.CrossProduct(localXVector, d);
            localXVector.Normalize();
            localYVector.Normalize();
            localZVector.Normalize();

            Vector3D moveVector = - mov.X * localXVector + mov.Y * localZVector;
            double moveLength = moveVector.Length;
            double moveRad = moveLength / 100;
            Vector3D moveDir = moveVector;
            moveDir.Normalize();

            Vector3D ncv = Math.Cos(moveRad) * cv + Math.Sin(moveRad) * fdist * moveDir;

            Vector3D np = fp + ncv;

            myPCamera.Position = new Point3D(np.X, np.Y, np.Z);

            //카메라 방향
            Vector3D nd = fp - np; //새 카메라 방향
            MyPCamera.LookDirection = nd;

            //카메라 upDirection 계산.
            //ncv 와 cv 사용에 따라서 달라짐
            Vector3D nu;
            //if (Math.Abs(mov.X) < Math.Abs(mov.Y))
            //{
            //    nu = Vector3D.CrossProduct(ncv, localXVector);
            //}
            //else
            //{
            //    nu = Vector3D.CrossProduct(cv, localXVector);
            //}
            nu = Vector3D.CrossProduct(ncv * Math.Abs(mov.Y) + cv * Math.Abs(mov.X), localXVector);
            //nu = Vector3D.CrossProduct(ncv, localXVector);
            myPCamera.UpDirection = nu;
        }
        internal void OrbitRotate007(Vector mov)
        {
            Point3D p = iniPCamera.Position;
            Vector3D pv = new Vector3D(p.X, p.Y, p.Z);  //카메라 위치
            Vector3D u = iniPCamera.UpDirection;
            Vector3D d = iniPCamera.LookDirection;

            //초점은 가시거리의 1/5로 잡는게 좋을 것 같음.
            d.Normalize();
            double fdist = iniPCamera.FarPlaneDistance / 5;
            Vector3D fp = pv + d * fdist; //초점
            Vector3D cv = pv - fp; //초점에 대한 카메라 위치 백터

            double rZ = -mov.X / 50;
            double rX = +mov.Y / 50;

            Vector3D localXVector = Vector3D.CrossProduct(d, u);
            Vector3D localZVector = Vector3D.CrossProduct(localXVector, d);
            localXVector.Normalize();
            localZVector.Normalize();

            Vector3D dz = Math.Cos(rX) * cv + Math.Sin(rX) * fdist * localZVector - cv;
            Vector3D dx = Math.Cos(rZ) * cv + Math.Sin(rZ) * fdist * localXVector - cv;

            Vector3D ncv = cv + dz + dx;
            Vector3D ncvn = ncv;
            ncvn.Normalize();
            ncv = ncvn * fdist;
            Vector3D np = fp + ncv;

            myPCamera.Position = new Point3D(np.X, np.Y, np.Z);

            //카메라 방향
            Vector3D nd = fp - np; //새 카메라 방향
            MyPCamera.LookDirection = nd;

            //카메라 upDirection 계산.
            //ncv 와 cv 사용에 따라서 달라짐
            Vector3D nu;
            //if (Math.Abs(mov.X) < Math.Abs(mov.Y))
            //{
            //    nu = Vector3D.CrossProduct(ncv, localXVector);
            //}
            //else
            //{
            //    nu = Vector3D.CrossProduct(cv, localXVector);
            //}
            nu = Vector3D.CrossProduct(ncv * Math.Abs(mov.Y) + cv * Math.Abs(mov.X), localXVector);
            myPCamera.UpDirection = nu;
        }

        internal void OrbitRotate006(Vector mov)
        {
            Point3D p = iniPCamera.Position;
            Vector3D pv = new Vector3D(p.X, p.Y, p.Z);  //카메라 위치
            Vector3D u = iniPCamera.UpDirection;
            Vector3D d = iniPCamera.LookDirection;

            //초점은 가시거리의 1/5로 잡는게 좋을 것 같음.
            d.Normalize();
            double fdist = iniPCamera.FarPlaneDistance / 5;
            Vector3D fp = pv + d * fdist; //초점
            Vector3D cv = pv - fp; //초점에 대한 카메라 위치 백터

            double rZ = -mov.X / 50;
            double rX = +mov.Y / 50;

            Vector3D localXVector = Vector3D.CrossProduct(d, u);
            Vector3D localZVector = Vector3D.CrossProduct(localXVector, d);
            localXVector.Normalize();
            localZVector.Normalize();

            Vector3D dz = Math.Cos(rX) * cv + Math.Sin(rX) * fdist * localZVector - cv;
            Vector3D dx = Math.Cos(rZ) * cv + Math.Sin(rZ) * fdist * localXVector - cv;

            Vector3D ncv = cv + dz + dx;
            Vector3D ncvn = ncv;
            ncvn.Normalize();
            ncv = ncvn * fdist;
            Vector3D np = fp + ncv;

            myPCamera.Position = new Point3D(np.X, np.Y, np.Z);

            //카메라 방향
            Vector3D nd = fp - np; //새 카메라 방향
            MyPCamera.LookDirection = nd;

            //카메라 upDirection 계산.
            //ncv 와 cv 사용에 따라서 달라짐
            // nu = Vector3D.CrossProduct(ncv, localXVector); // 이걸로 계산하면 위아래 방향 회전이 잘 나오고.
            // nu = Vector3D.CrossProduct(cv, localXVector); // 이걸로 계산하면 좌우방향 회전이 잘 나옴.
            // 좌우나 위아래 방향에 폭에 따라서 ncv와 cv의 영향을 다르게 주면 둘 다 나름 잘 맞게됨.
            // upDir이 잘 안맞는 경우 도형이 마구 회전하게됨. 맞추기 쉽지 않음. 200328
            Vector3D nu = Vector3D.CrossProduct(cv * mov.Y + ncv * mov.X, localXVector);
            myPCamera.UpDirection = nu;
        }
        internal void OrbitRotate005(Vector mov)
        {
            Point3D p = iniPCamera.Position;
            Vector3D pv = new Vector3D(p.X, p.Y, p.Z);  //카메라 위치
            Vector3D u = iniPCamera.UpDirection;
            Vector3D d = iniPCamera.LookDirection;

            //초점은 가시거리의 1/3로 잡는게 좋을 것 같음.
            d.Normalize();
            double fdist = iniPCamera.FarPlaneDistance / 3;
            Vector3D fp = pv + d * fdist; //초점
            Vector3D cv = pv - fp; //초점에 대한 카메라 위치 백터

            double rZ = -mov.X / 40;
            double rX = +mov.Y / 40;

            Vector3D localXVector = Vector3D.CrossProduct(d, u);
            Vector3D localYVector = d;
            Vector3D localZVector = Vector3D.CrossProduct(localXVector, d);
            localXVector.Normalize();
            localYVector.Normalize();
            localZVector.Normalize();

            Vector3D dz = Math.Cos(rX) * cv + Math.Sin(rX) * fdist * localZVector - cv;
            Vector3D dx = Math.Cos(rZ) * cv + Math.Sin(rZ) * fdist * localXVector - cv;
            //dx = localXVector * Math.Tan(rZ) * cv.Length;

            Vector3D ncv = cv + dz + dx;
            ncv.Normalize();
            Vector3D np = fp + ncv * fdist;

            myPCamera.Position = new Point3D(np.X, np.Y, np.Z);

            //카메라 방향
            Vector3D nd = fp - np; //새 카메라 방향
            MyPCamera.LookDirection = nd;

            //카메라 upDirection 계산.
            //ncv 와 cv 사용에 따라서 달라짐
            Vector3D nu;
            if (Math.Abs(mov.X) < Math.Abs(mov.Y))
            {
                nu = Vector3D.CrossProduct(ncv, localXVector);
            }
            else
            {
                nu = Vector3D.CrossProduct(cv, localXVector);
            }
            myPCamera.UpDirection = nu;
        }
        internal void OrbitRotate004(Vector mov)
        {
            Point3D p = iniPCamera.Position;
            Vector3D pv = new Vector3D(p.X, p.Y, p.Z);  //카메라 위치
            Vector3D u = iniPCamera.UpDirection;
            Vector3D d = iniPCamera.LookDirection;

            //초점은 가시거리의 1/3로 잡는게 좋을 것 같음.
            d.Normalize();
            double fdist = iniPCamera.FarPlaneDistance / 3;
            Vector3D fp = pv + d * fdist; //초점
            Vector3D cv = pv - fp; //초점에 대한 카메라 위치 백터

            double rZ = -mov.X / 40;
            double rX = +mov.Y / 40;

            Vector3D localXVector = Vector3D.CrossProduct(d, u);
            Vector3D localYVector = d;
            Vector3D localZVector = Vector3D.CrossProduct(localXVector, d);
            localXVector.Normalize();
            localYVector.Normalize();
            localZVector.Normalize();

            Vector3D dz = localZVector * Math.Tan(rX) * cv.Length;
            Vector3D dx = localXVector * Math.Tan(rZ) * cv.Length;
            //Vector3D dz = (localZVector * Math.Cos(rX) + localYVector * Math.Sin(rX)) * cv.Length;
            //Vector3D dx = (- localXVector * Math.Cos(rZ) + localYVector * Math.Sin(rZ)) * cv.Length;

            Vector3D ncv = cv + dz + dx;
            ncv.Normalize();
            Vector3D np = fp + ncv * fdist;

            myPCamera.Position = new Point3D(np.X, np.Y, np.Z);

            //카메라 방향
            Vector3D nd = fp - np; //새 카메라 방향
            MyPCamera.LookDirection = nd;

            //카메라 위.
            Vector3D nu;
            if (rZ > rX)
            {
                nu = Vector3D.CrossProduct(localXVector, localZVector);
            }
            else
            {
                nu = Vector3D.CrossProduct(Vector3D.CrossProduct(ncv, cv),ncv);
            }
            myPCamera.UpDirection = nu;
        }
        internal void OrbitRotate003(Vector mov)
        {
            Point3D p = iniPCamera.Position;
            Vector3D pv = new Vector3D(p.X, p.Y, p.Z);  //카메라 위치
            Vector3D u = iniPCamera.UpDirection;
            Vector3D d = iniPCamera.LookDirection;

            //초점은 가시거리의 1/3로 잡는게 좋을 것 같음.
            Vector3D norLookDirection = d;
            norLookDirection.Normalize();
            double fdist = iniPCamera.FarPlaneDistance / 3;
            Vector3D fp = pv + norLookDirection * fdist; //초점

            Vector3D cv = pv - fp; //초점에 대한 카메라 위치 백터

            double rZ = -mov.X / 40;
            double rX = +mov.Y / 40;

            Vector3D localXVector = Vector3D.CrossProduct(norLookDirection, u);
            Vector3D localYVector = norLookDirection;
            Vector3D localZVector = Vector3D.CrossProduct(localXVector, norLookDirection);
            localXVector.Normalize();
            localYVector.Normalize();
            localZVector.Normalize();

            Vector3D dz = localZVector * Math.Tan(rX) * cv.Length;
            Vector3D dx = localXVector * Math.Tan(rZ) * cv.Length;

            Vector3D ncv = cv + dz + dx;
            ncv.Normalize();
            Vector3D np = fp + ncv * fdist;

            myPCamera.Position = new Point3D(np.X, np.Y, np.Z);

            //카메라 방향
            Vector3D nd = fp - np; //새 카메라 방향
            MyPCamera.LookDirection = nd;

            //카메라 위.
            Vector3D nu = Vector3D.CrossProduct(cv, localXVector);
            myPCamera.UpDirection = nu;
        }
        internal void OrbitRotate002(Vector mov)
        {
            Point3D p = iniPCamera.Position;
            Vector3D pv = new Vector3D  //카메라 위치
            {
                X = p.X,
                Y = p.Y,
                Z = p.Z
            };

            //초점은 가시거리의 1/3로 잡는게 좋을 것 같음.
            Vector3D norLookDirection = new Vector3D();
            norLookDirection = iniPCamera.LookDirection;
            norLookDirection.Normalize();

            Vector3D fp = pv + norLookDirection * iniPCamera.FarPlaneDistance / 3; //초점
            //fp = pv + iniPCamera.LookDirection; //초점

            Vector3D cv = pv - fp; //초점에 대한 카메라 위치 백터
            Vector3D np = new Vector3D(); //새 카메라 위치

            //회전좌표계로 변환
            double r = cv.Length;
            double irz;
            double iry;
            if (cv.X != 0)
            {
                irz = Math.Atan2(cv.Y, cv.X);
                iry = Math.Atan2(cv.Z, cv.X / Math.Cos(irz)); //회전좌표(위도). 투영과는 값이 다름.
            }
            else
            {
                irz = Math.Atan2(cv.Y, 0);
                iry = Math.Atan2(cv.Z, 0); //회전좌표(위도). 투영과는 값이 다름.
            }

            double rY = mov.X / 40;
            double rZ = mov.Y / 40;

            np.X = r;
            np.Y = 0;
            np.Z = 0;

            Vector3D np1 = new Vector3D();

            np1.X = np.X * Math.Cos(rZ) - np.Y * Math.Sin(rZ);
            np1.Y = np.X * Math.Sin(rZ) + np.Y * Math.Cos(rZ);
            np1.Z = np.Z;

            np.X = np1.X * Math.Cos(rY) - np1.Z * Math.Sin(rY);
            np.Y = np1.Y;
            np.Z = np1.X * Math.Sin(rY) + np1.Z * Math.Cos(rY);

            np1.X = np.X * Math.Cos(iry) - np.Z * Math.Sin(iry);
            np1.Y = np.Y;
            np1.Z = np.X * Math.Sin(iry) + np.Z * Math.Cos(iry);

            np.X = np1.X * Math.Cos(irz) - np1.Y * Math.Sin(irz);
            np.Y = np1.X * Math.Sin(irz) + np1.Y * Math.Cos(irz);
            np.Z = np1.Z;

            np += fp;

            p.X = np.X;
            p.Y = np.Y;
            p.Z = np.Z;
            myPCamera.Position = p;

            //카메라 방향
            Vector3D nd = fp - np; //새 카메라 방향
            MyPCamera.LookDirection = nd;
        }
        internal void OrbitRotate001(Vector mov)
        {
            Point3D p = iniPCamera.Position;
            Vector3D pv = new Vector3D  //카메라 위치
            {
                X = p.X,
                Y = p.Y,
                Z = p.Z
            };

            //초점은 가시거리의 1/3로 잡는게 좋을 것 같음.
            Vector3D norLookDirection = new Vector3D();
            norLookDirection = iniPCamera.LookDirection;
            norLookDirection.Normalize();

            Vector3D fp = pv + norLookDirection * iniPCamera.FarPlaneDistance / 10; //초점

            Vector3D cv = pv - fp; //초점에 대한 카메라 위치 백터
            Vector3D np = new Vector3D(); //새 카메라 위치

            //회전좌표계로 변환
            double r = cv.Length;
            double irz;
            double iry;
            if (cv.X != 0)
            {
                irz = Math.Atan2(cv.Y, cv.X);
                iry = Math.Atan2(cv.Z, cv.X / Math.Cos(irz)); //회전좌표(위도). 투영과는 값이 다름.
            }
            else
            {
                irz = Math.Atan2(cv.Y, 0);
                iry = Math.Atan2(cv.Z, 0); //회전좌표(위도). 투영과는 값이 다름.
            }

            double rY = mov.X / 40;
            double rZ = mov.Y / 40;

            np.X = r;
            np.Y = 0;
            np.Z = 0;

            Vector3D np1 = new Vector3D();

            np1.X = np.X * Math.Cos(rZ) - np.Y * Math.Sin(rZ);
            np1.Y = np.X * Math.Sin(rZ) + np.Y * Math.Cos(rZ);
            np1.Z = np.Z;

            np.X = np1.X * Math.Cos(rY) - np1.Z * Math.Sin(rY);
            np.Y = np1.Y;
            np.Z = np1.X * Math.Sin(rY) + np1.Z * Math.Cos(rY);

            np1.X = np.X * Math.Cos(iry) - np.Z * Math.Sin(iry);
            np1.Y = np.Y;
            np1.Z = np.X * Math.Sin(iry) + np.Z * Math.Cos(iry);

            np.X = np1.X * Math.Cos(irz) - np1.Y * Math.Sin(irz);
            np.Y = np1.X * Math.Sin(irz) + np1.Y * Math.Cos(irz);
            np.Z = np1.Z;

            np += fp;

            p.X = np.X;
            p.Y = np.Y;
            p.Z = np.Z;
            myPCamera.Position = p;

            //카메라 방향
            Vector3D nd = fp - np; //새 카메라 방향
            MyPCamera.LookDirection = nd;
        }
        internal void OrbitRotate000(Vector mov)
        {
            Point3D p = iniPCamera.Position;
            Vector3D pv = new Vector3D  //카메라 위치
            {
                X = p.X,
                Y = p.Y,
                Z = p.Z
            };

            Vector3D fp = pv + iniPCamera.LookDirection; //초점
            Vector3D cv = pv - fp; //초점에 대한 카메라 위치 백터
            Vector3D np = new Vector3D(); //새 카메라 위치

            //회전좌표계로 변환
            double r = cv.Length;
            double irz;
            double iry;
            if (cv.X != 0)
            {
                irz = Math.Atan2(cv.Y, cv.X);
                iry = Math.Atan2(cv.Z, cv.X / Math.Cos(irz)); //회전좌표(위도). 투영과는 값이 다름.
            }
            else
            {
                irz = Math.Atan2(cv.Y, 0);
                iry = Math.Atan2(cv.Z, 0); //회전좌표(위도). 투영과는 값이 다름.
            }

            double rY = mov.X / 40;
            double rZ = mov.Y / 40;

            np.X = r;
            np.Y = 0;
            np.Z = 0;

            Vector3D np1 = new Vector3D();

            np1.X = np.X * Math.Cos(rZ) - np.Y * Math.Sin(rZ);
            np1.Y = np.X * Math.Sin(rZ) + np.Y * Math.Cos(rZ);
            np1.Z = np.Z;

            np.X = np1.X * Math.Cos(rY) - np1.Z * Math.Sin(rY);
            np.Y = np1.Y;
            np.Z = np1.X * Math.Sin(rY) + np1.Z * Math.Cos(rY);

            np1.X = np.X * Math.Cos(iry) - np.Z * Math.Sin(iry);
            np1.Y = np.Y;
            np1.Z = np.X * Math.Sin(iry) + np.Z * Math.Cos(iry);

            np.X = np1.X * Math.Cos(irz) - np1.Y * Math.Sin(irz);
            np.Y = np1.X * Math.Sin(irz) + np1.Y * Math.Cos(irz);
            np.Z = np1.Z;

            np += fp;

            p.X = np.X;
            p.Y = np.Y;
            p.Z = np.Z;
            myPCamera.Position = p;

            //카메라 방향
            Vector3D nd = fp - np; //새 카메라 방향
            MyPCamera.LookDirection = nd;
        }

        internal void OrbitMove001(Vector vector)
        {
            Point3D p = iniPCamera.Position;
            Vector3D d = iniPCamera.LookDirection;
            Vector3D u = iniPCamera.UpDirection;

            Vector3D rightVector = Vector3D.CrossProduct(d, u);
            Vector3D upVector = Vector3D.CrossProduct(rightVector, d);
            rightVector.Normalize();
            upVector.Normalize();

            Vector3D pv = new Vector3D(p.X, p.Y, p.Z);
            Vector3D np = pv - rightVector * vector.X / 8 + upVector * vector.Y / 8;

            myPCamera.Position = new Point3D(np.X,np.Y,np.Z);
        }
        internal void OrbitMove(Vector vector)
        {
            Point3D p = iniPCamera.Position;
            p.X = iniPCamera.Position.X - vector.X / 8;
            p.Y = iniPCamera.Position.Y + vector.Y / 8;
            myPCamera.Position = p;
        }

        internal void OrbitStart()
        {
            iniPCamera = new PerspectiveCamera
            {
                //속성값들을 복사해주어야 깊은 복사가 됨. 복사를 안한 값들은 참조로 됨.
                Position = myPCamera.Position,
                LookDirection = myPCamera.LookDirection
            };
            iniPCamera.UpDirection = MyPCamera.UpDirection;
            iniPCamera.FarPlaneDistance = MyPCamera.FarPlaneDistance;
        }
        internal void OrbitMoveX001(double x)
        {
            Point3D p = iniPCamera.Position;
            Vector3D d = iniPCamera.LookDirection;
            Vector3D u = iniPCamera.UpDirection;

            Vector3D rightVector = Vector3D.CrossProduct(d, u);
            Vector3D upVector = Vector3D.CrossProduct(rightVector, d);

            Vector3D pv = new Vector3D(p.X, p.Y, p.Z);
            
                       
            p.X = iniPCamera.Position.X + x;
            myPCamera.Position = p;
        }
        internal void OrbitMoveX(double x)
        {
            Point3D p = iniPCamera.Position;
            p.X = iniPCamera.Position.X + x;
            myPCamera.Position = p;
        }
        internal void OrbitMoveY(double y)
        {
            Point3D p = iniPCamera.Position;
            p.Y = iniPCamera.Position.Y + y;
            myPCamera.Position = p;
        }
        internal void OrbitEnd()
        {
            //iniPCamera = myPCamera;
        }

 
    }
    class BckDrawing
    {
        private Grid grid;
        private readonly int iniLineThickness = 2;
        public BckDrawing(Grid grid)
        {
            this.Grid = grid;
        }
        public Grid Grid { get => grid; set => grid = value; }
        public void DrawSampleGradPlan()
        {
            // Declare scene objects.
            Viewport3D myViewport3D = new Viewport3D();
            Model3DGroup myModel3DGroup = new Model3DGroup();
            GeometryModel3D myGeometryModel = new GeometryModel3D();
            ModelVisual3D myModelVisual3D = new ModelVisual3D();
            // Defines the camera used to view the 3D object. In order to view the 3D object,
            // the camera must be positioned and pointed such that the object is within view 
            // of the camera.
            PerspectiveCamera myPCamera = new PerspectiveCamera
            {
                // Specify where in the 3D scene the camera is.
                Position = new Point3D(0, 0, 2),

                // Specify the direction that the camera is pointing.
                LookDirection = new Vector3D(0, 0, -1),

                // Define camera's horizontal field of view in degrees.
                FieldOfView = 60
            };

            // Asign the camera to the viewport
            myViewport3D.Camera = myPCamera;
            // Define the lights cast in the scene. Without light, the 3D object cannot 
            // be seen. Note: to illuminate an object from additional directions, create 
            // additional lights.
            DirectionalLight myDirectionalLight = new DirectionalLight
            {
                Color = Colors.White,
                Direction = new Vector3D(-0.61, -0.5, -0.61)
            };

            myModel3DGroup.Children.Add(myDirectionalLight);

            // The geometry specifes the shape of the 3D plane. In this sample, a flat sheet 
            // is created.
            MeshGeometry3D myMeshGeometry3D = new MeshGeometry3D();

            // Create a collection of normal vectors for the MeshGeometry3D.
            Vector3DCollection myNormalCollection = new Vector3DCollection
            {
                new Vector3D(0, 0, 1),
                new Vector3D(0, 0, 1),
                new Vector3D(0, 0, 1),
                new Vector3D(0, 0, 1),
                new Vector3D(0, 0, 1),
                new Vector3D(0, 0, 1)
            };
            myMeshGeometry3D.Normals = myNormalCollection;

            // Create a collection of vertex positions for the MeshGeometry3D. 
            Point3DCollection myPositionCollection = new Point3DCollection
            {
                new Point3D(-0.5, -0.5, 0.5),
                new Point3D(0.5, -0.5, 0.5),
                new Point3D(0.5, 0.5, 0.5),
                new Point3D(0.5, 0.5, 0.5),
                new Point3D(-0.5, 0.5, 0.5),
                new Point3D(-0.5, -0.5, 0.5)
            };
            myMeshGeometry3D.Positions = myPositionCollection;

            // Create a collection of texture coordinates for the MeshGeometry3D.
            PointCollection myTextureCoordinatesCollection = new PointCollection
            {
                new Point(0, 0),
                new Point(1, 0),
                new Point(1, 1),
                new Point(1, 1),
                new Point(0, 1),
                new Point(0, 0)
            };
            myMeshGeometry3D.TextureCoordinates = myTextureCoordinatesCollection;

            // Create a collection of triangle indices for the MeshGeometry3D.
            Int32Collection myTriangleIndicesCollection = new Int32Collection
            {
                0,
                1,
                2,
                3,
                4,
                5
            };
            myMeshGeometry3D.TriangleIndices = myTriangleIndicesCollection;

            // Apply the mesh to the geometry model.
            myGeometryModel.Geometry = myMeshGeometry3D;

            // The material specifies the material applied to the 3D object. In this sample a  
            // linear gradient covers the surface of the 3D object.

            // Create a horizontal linear gradient with four stops.   
            LinearGradientBrush myHorizontalGradient = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0.5),
                EndPoint = new Point(1, 0.5)
            };
            myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.Yellow, 0.0));
            myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.Red, 0.25));
            myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.Blue, 0.75));
            myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.LimeGreen, 1.0));

            // Define material and apply to the mesh geometries.
            DiffuseMaterial myMaterial = new DiffuseMaterial(myHorizontalGradient);
            myGeometryModel.Material = myMaterial;

            // Apply a transform to the object. In this sample, a rotation transform is applied,  
            // rendering the 3D object rotated.
            RotateTransform3D myRotateTransform3D = new RotateTransform3D();
            AxisAngleRotation3D myAxisAngleRotation3d = new AxisAngleRotation3D
            {
                Axis = new Vector3D(0, 3, 0),
                Angle = 40
            };
            myRotateTransform3D.Rotation = myAxisAngleRotation3d;
            myGeometryModel.Transform = myRotateTransform3D;

            // Add the geometry model to the model group.
            myModel3DGroup.Children.Add(myGeometryModel);

            // Add the group of models to the ModelVisual3d.
            myModelVisual3D.Content = myModel3DGroup;

            // 
            myViewport3D.Children.Add(myModelVisual3D);

            // Apply the viewport to the page so it will be rendered.
            //this.Content = myViewport3D;
            grid.Children.Add(myViewport3D);

            //grdBackground.Visibility = Visibility.Hidden;





        }
        internal void DrawSampleMesh()
        {

            MeshGeometry3D triangleMesh = new MeshGeometry3D();
            Point3D point0 = new Point3D(0, 0, 0);
            Point3D point1 = new Point3D(5, 0, 0);
            Point3D point2 = new Point3D(0, 0, 5);

            triangleMesh.Positions.Add(point0);
            triangleMesh.Positions.Add(point1);
            triangleMesh.Positions.Add(point2);

            triangleMesh.TriangleIndices.Add(0);
            triangleMesh.TriangleIndices.Add(2);
            triangleMesh.TriangleIndices.Add(1);

            Vector3D normal = new Vector3D(0, 1, 0);
            triangleMesh.Normals.Add(normal);
            triangleMesh.Normals.Add(normal);
            triangleMesh.Normals.Add(normal);

            Material material = new DiffuseMaterial(new SolidColorBrush(Colors.DarkKhaki));
            GeometryModel3D triangleModel = new GeometryModel3D(triangleMesh, material);

            Model3DGroup modelGroup = new Model3DGroup();
            modelGroup.Children.Add(triangleModel);


            Viewport3D myViewport3D = new Viewport3D();
            PerspectiveCamera myPCamera = new PerspectiveCamera
            {
                // Specify where in the 3D scene the camera is.
                Position = new Point3D(11, 10, 9),

                // Specify the direction that the camera is pointing.
                LookDirection = new Vector3D(-11, -10, -9),

                // Define camera's horizontal field of view in degrees.
                FieldOfView = 70,
                FarPlaneDistance = 100,
                UpDirection = new Vector3D(0, 1, 0),
                NearPlaneDistance = 1
            };

            // Asign the camera to the viewport
            myViewport3D.Camera = myPCamera;

            DirectionalLight myDirLight = new DirectionalLight
            {
                Color = Colors.White,
                Direction = new Vector3D(-3, -4, -5)
            };

            modelGroup.Children.Add(myDirLight);

            ModelVisual3D model = new ModelVisual3D
            {
                Content = modelGroup
            };


            myViewport3D.Children.Add(model);

            // Apply the viewport to the page so it will be rendered.
            //this.Content = myViewport3D;
            grid.Children.Add(myViewport3D);

            //myPCamera.FarPlaneDistance = 15;

        }
        internal void DrawLine(double x1, double x2, double y1, double y2)
        {
            DrawLine(x1, x2, y1, y2, iniLineThickness);
        }
        internal void DrawLine(double x1, double x2, double y1, double y2, double thickness)
        {
            Line myLine = new Line
            {
                Stroke = System.Windows.Media.Brushes.LightSteelBlue,
                X1 = x1,
                X2 = x2,
                Y1 = y1,
                Y2 = y2,
                //HorizontalAlignment = HorizontalAlignment.Left,
                //VerticalAlignment = VerticalAlignment.Center,
                StrokeThickness = thickness
            };
            grid.Children.Add(myLine);
        }
        internal void DrawSampleLine()
        {
            // Add a Line Element
            Line myLine = new Line
            {
                Stroke = System.Windows.Media.Brushes.LightSteelBlue,
                X1 = 1,
                X2 = 50,
                Y1 = 1,
                Y2 = 100,
                //HorizontalAlignment = HorizontalAlignment.Left,
                //VerticalAlignment = VerticalAlignment.Center,
                StrokeThickness = 5
            };
            grid.Children.Add(myLine);
            //myLine.X1 = 100;

        }
    }
}
