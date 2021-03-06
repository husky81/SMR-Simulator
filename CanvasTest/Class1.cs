﻿using bck.SMR_simulator.general_functions;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace CanvasTest
{
    public partial class Draw3D // 기본
    {
        public readonly Viewport3D grid;

        public Shapes3D shapes;
        public TextShapes3D texts = new TextShapes3D();
        private ModelVisual3D modelVisual3d_Shapes;
        private ModelVisual3D modelVisual3d_Texts;

        /// <summary>
        /// 노드 추가할 때 졸졸 따라다니는 sphere.
        /// </summary>
        public PointMarker3D pointMarker;

        internal SelectionWindow selectionWindow;
        public void RedrawUiShapes()
        {
            grid.Children.Clear();
            foreach (Rectangle rectangle in selectionWindow.shapes.rectangles)
            {
                grid.Children.Add(rectangle);
            }
        }

        internal void SetBasePlaneGrid()
        {
            Vector3D normalVector = new Vector3D(0, 0, 1);
            Vector3D localX = new Vector3D(1, 0, 0);
            Point3D center = new Point3D(0, 0, 0);

            double subGap = 1.0d;
            int mainFrequent = 5;
            int numSubbarHalf = 50;
            double length = subGap * numSubbarHalf * 2;

            double diaMain = 0.02;
            double diaSub = 0.01;
            //diaMain = 0.2;
            //diaSub = 0.1;
            int resolution = 2;
            double opacityMain = 0.3;
            double opacitySub = 0.15;

            Color color = Colors.Gray;

            //Shapes.Add 메서드를 사용하는 경우 동일한 메쉬를 반복해서 생성하게되므로 다시 만듦
            MeshGeometry3D meshMain = MeshGenerator.Cylinder(diaMain, length, resolution);
            MeshGeometry3D meshSub = MeshGenerator.Cylinder(diaSub, length, resolution);

            Vector3D axisY = new Vector3D(0, 1, 0);
            Vector3D axisZ = new Vector3D(0, 0, 1);

            RotateTransform3D rotateTransform3dZ = new RotateTransform3D(new AxisAngleRotation3D(axisZ, 90));
            RotateTransform3D rotateTransform3dY = new RotateTransform3D(new AxisAngleRotation3D(axisY, 90));
            RotateTransform3D rotateTransform3dZ2 = new RotateTransform3D(new AxisAngleRotation3D(axisZ, 0));
            Model3DGroup modelGroup = new Model3DGroup();
            //modelGroup.Children.Add(myDirLight);
            for (int i = -numSubbarHalf; i <= numSubbarHalf; i++)
            {
                Shape3D c = new Shape3D();
                if (i % mainFrequent == 0)
                {
                    c.mesh = meshMain;
                    c.Opacity(opacityMain);
                }
                else
                {
                    c.mesh = meshSub;
                    c.Opacity(opacitySub);
                }
                c.rotateTransform3dZ = rotateTransform3dZ;
                c.rotateTransform3dY = rotateTransform3dY;
                c.rotateTransform3dZ2 = rotateTransform3dZ2;
                c.translateTransform3D = new TranslateTransform3D(new Vector3D(-length / 2, subGap * i, 0));
                c.Color(color);
                modelGroup.Children.Add(c.GeoModel());
            }

            //rotateTransform3dZ = new RotateTransform3D(new AxisAngleRotation3D(axisZ, 90));
            //rotateTransform3dY = new RotateTransform3D(new AxisAngleRotation3D(axisY, 90));
            rotateTransform3dZ2 = new RotateTransform3D(new AxisAngleRotation3D(axisZ, 90));
            for (int i = -numSubbarHalf; i <= numSubbarHalf; i++)
            {
                Shape3D c = new Shape3D();
                if (i % mainFrequent == 0)
                {
                    c.mesh = meshMain;
                    c.Opacity(opacityMain);
                }
                else
                {
                    c.mesh = meshSub;
                    c.Opacity(opacitySub);
                }
                c.rotateTransform3dZ = rotateTransform3dZ;
                c.rotateTransform3dY = rotateTransform3dY;
                c.rotateTransform3dZ2 = rotateTransform3dZ2;
                c.translateTransform3D = new TranslateTransform3D(new Vector3D(subGap * i, -length / 2, 0));
                c.Color(color);
                modelGroup.Children.Add(c.GeoModel());
            }

            modelVisual_BasePlaneGrid = new ModelVisual3D
            {
                Content = modelGroup
            };
        }
        public bool showBasePlaneGrid = true;
        ModelVisual3D modelVisual_BasePlaneGrid = new ModelVisual3D();

        internal void SetCoordinateSystem()
        {
            double length = 1;

            Point3D p0 = new Point3D(0, 0, 0);
            Point3D pX = new Point3D(length, 0, 0);
            Point3D pY = new Point3D(0, length, 0);
            Point3D pZ = new Point3D(0, 0, length);

            //3D Line
            //Shapes ss = new Shapes();
            //ss.AddLine(p0, pX);
            //ss.recentShape.Color(Colors.Red);
            //ss.AddLine(p0, pY);
            //ss.recentShape.Color(Colors.Green);
            //ss.AddLine(p0, pZ);
            //ss.recentShape.Color(Colors.Blue);

            //Cylinder
            Shapes3D ss = new Shapes3D();
            double dia = 0.1;
            int resolution = 16;
            Cylinder3D xAxis = ss.AddCylinder(p0, pX - p0, dia, resolution);
            Cylinder3D yAxis = ss.AddCylinder(p0, pY - p0, dia, resolution);
            Cylinder3D zAxis = ss.AddCylinder(p0, pZ - p0, dia, resolution);
            xAxis.Close();
            yAxis.Close();
            zAxis.Close();
            xAxis.Color(Colors.Red);
            yAxis.Color(Colors.Green);
            zAxis.Color(Colors.Blue);
            xAxis.Opacity(0.5);
            yAxis.Opacity(0.5);
            zAxis.Opacity(0.5);

            Model3DGroup modelGroup = new Model3DGroup();
            foreach (Shape3D s in ss)
            {
                modelGroup.Children.Add(s.GeoModel());
            }
            modelVisual_CoordinateSystem = new ModelVisual3D
            {
                Content = modelGroup
            };
        }
        public bool showCoordinateSystem = true;
        ModelVisual3D modelVisual_CoordinateSystem = new ModelVisual3D();

        private readonly Model3DGroup model3DGroup = new Model3DGroup();
        private readonly Viewport3D viewport = new Viewport3D();

        DirectionalLight myDirLight = new DirectionalLight
        {
            Color = Colors.White,
            Direction = new Vector3D(3, 4, -5)
        };

        public Draw3D(Viewport3D grid)
        {
            //생성자
            this.grid = grid;
            shapes = new Shapes3D();
            viewport.Camera = PCamera;

            SetCoordinateSystem();
            SetBasePlaneGrid();
            pointMarker = new PointMarker3D(this);
            selectionWindow = new SelectionWindow(grid);

            RegenerateShapes_ModelVisual3ds();
            RedrawShapes();
        }
        public void RedrawShapes()
        {
            model3DGroup.Children.Clear();
            model3DGroup.Children.Add(myDirLight);
            ModelVisual3D modelVisual = new ModelVisual3D();
            modelVisual.Content = model3DGroup;

            viewport.Children.Clear();
            viewport.Children.Add(modelVisual);

            viewport.Children.Add(modelVisual3d_Shapes);
            viewport.Children.Add(modelVisual3d_Texts);
            if (showBasePlaneGrid) viewport.Children.Add(modelVisual_BasePlaneGrid);
            if (showCoordinateSystem) viewport.Children.Add(modelVisual_CoordinateSystem);
            if (pointMarker.visibility) viewport.Children.Add(pointMarker.modelVisual3D);

            grid.Children.Clear();
            grid.Children.Add(viewport);
        }
        public void RegenerateShapes_ModelVisual3ds()
        {
            modelVisual3d_Shapes = shapes.ModelVisual3D();
            modelVisual3d_Texts = texts.ModelVisual3D();
        }

        /// <summary>
        /// grid의 2차원 좌표를 3차원으로 변환.
        /// 반환되는 좌표는 BasePlaneGrid에 있음.
        /// </summary>
        /// <param name="p0"></param>
        /// <returns></returns>
        public Point3D GetPoint3dOnBasePlane_FromPoint2D(Point p0)
        {
            //Grid의 마우스 포인트
            double gridHeight = grid.ActualHeight;
            double gridWidth = grid.ActualWidth;
            Point gridCenter = new Point(gridWidth / 2, gridHeight / 2);
            Vector p = p0 - gridCenter;

            //베이스평면 정보
            Point3D ucsBasePoint = new Point3D(0, 0, 0);
            Vector3D ucsX = new Vector3D(1, 0, 0);
            Vector3D ucsY = new Vector3D(0, 1, 0);
            Vector3D ucsNormal = Vector3D.CrossProduct(ucsX, ucsY);

            //카메라 정보
            double fov = PCamera.FieldOfView;
            Point3D pos = PCamera.Position;
            Vector3D dir = PCamera.LookDirection;
            Vector3D up = PCamera.UpDirection;

            //카메라 관련 벡터 정규화
            Vector3D camY = Vector3D.CrossProduct(up, dir);
            up = Vector3D.CrossProduct(dir, camY);
            camY.Normalize();
            up.Normalize();
            dir.Normalize();

            //FieldOfView 기준으로 위치 환산
            double pointRatio2dTo3d = Math.Tan(fov / 180 * Math.PI / 2) / (gridWidth / 2);
            double rightTan = pointRatio2dTo3d * p.X;
            double upTan = pointRatio2dTo3d * p.Y;

            //카메라 위치에서 마우스포인터가 가리키는 방향의 벡터 산정
            Vector3D pointDirection = -rightTan * camY - upTan * up + dir;

            //pointDirection.Normalize(); //이 벡터는 정규화 안해도 됨.

            //카메라와 절점의 거리
            //ref. http://www.gisdeveloper.co.kr/?p=792
            double u = Vector3D.DotProduct(ucsNormal, ucsBasePoint - pos) / Vector3D.DotProduct(ucsNormal, pointDirection);

            Point3D crossPoint = pos + u * pointDirection; //마우스커서 위치와 basePlane의 접점
            return crossPoint;
        }
        /// <summary>
        /// grid의 2차원 좌표를 3차원으로 변환.
        /// 반환되는 좌표는 카메라 앞에 있음.
        /// </summary>
        /// <param name="p0"></param>
        /// <returns></returns>
        public Point3D GetPoint3d_FromPoint2D(Point p0)
        {
            //Grid의 마우스 포인트
            double gridHeight = grid.ActualHeight;
            double gridWidth = grid.ActualWidth;
            Point gridCenter = new Point(gridWidth / 2, gridHeight / 2);
            Vector p = p0 - gridCenter;

            //카메라 정보
            double fov = PCamera.FieldOfView;
            Point3D pos = PCamera.Position;
            Vector3D dir = PCamera.LookDirection;
            Vector3D up = PCamera.UpDirection;

            //카메라 관련 벡터 정규화
            Vector3D camY = Vector3D.CrossProduct(up, dir);
            up = Vector3D.CrossProduct(dir, camY);
            camY.Normalize();
            up.Normalize();
            dir.Normalize();

            //FieldOfView 기준으로 위치 환산
            double pointRatio2dTo3d = Math.Tan(fov / 180 * Math.PI / 2) / (gridWidth / 2);
            double rightTan = pointRatio2dTo3d * p.X;
            double upTan = pointRatio2dTo3d * p.Y;

            //카메라 위치에서 마우스포인터가 가리키는 방향의 벡터 산정
            Vector3D pointDirection = -rightTan * camY - upTan * up + dir;
            pointDirection.Normalize();

            Point3D crossPoint = pos + pointDirection; //마우스커서 위치
            return crossPoint;
        }
        /// <summary>
        /// 3차원 점에 대응하는 grid의 좌표
        /// </summary>
        /// <param name="p0"></param>
        /// <returns></returns>
        public Point GetPoint2D_FromPoint3D(Point3D p0)
        {
            Point3D pos = PCamera.Position;
            Vector3D dir = PCamera.LookDirection;
            Vector3D up = PCamera.UpDirection;
            dir.Normalize();
            up.Normalize();
            Vector3D camY = Vector3D.CrossProduct(up, dir);
            up = Vector3D.CrossProduct(dir, camY);
            up.Normalize();
            camY.Normalize();

            double width = grid.ActualWidth;
            double height = grid.ActualHeight;

            double fovW = PCamera.FieldOfView;
            //double fovH = Math.Atan(Math.Tan(fovW / 2 / 180 * Math.PI) / width * height) * 2 * 180 / Math.PI;

            Vector3D pointVector = p0 - pos;
            Point3D pV = pointVector + new Point3D(0, 0, 0);
            double dist = GF.PlanePosition(dir, pV);
            double unitX = -GF.PlanePosition(camY, pV) / dist;
            double unitY = -GF.PlanePosition(up, pV) / dist;

            double angX = Math.Atan2(unitX, 1);
            double angY = Math.Atan2(unitY, 1);

            double resolution = width / 2 / Math.Tan(fovW / 180 * Math.PI / 2);

            double x = width / 2 + Math.Tan(angX) * resolution;
            double y = height / 2 + Math.Tan(angY) * resolution;

            Point outP = new Point(x, y);
            return outP;
        }

        private readonly int iniLine2DThickness = 2;
        internal void DrawLine2D(Point p0, Point p1, double thickness)
        {
            Line myLine = new Line
            {
                Stroke = System.Windows.Media.Brushes.LightSteelBlue,
                X1 = p0.X,
                X2 = p1.X,
                Y1 = p0.Y,
                Y2 = p1.Y,
                //HorizontalAlignment = HorizontalAlignment.Left,
                //VerticalAlignment = VerticalAlignment.Center,
                StrokeThickness = thickness
            };
            grid.Children.Add(myLine);
        }
        internal void DrawLine2D(Point p0, Point p1)
        {
            DrawLine2D(p0, p1, iniLine2DThickness);
        }

        public void DrawGradient_Sample()
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

            //grdBackground.Visibility = Visibility.Hidden;

        }
        internal void DrawLine3D_Sample(Point3D p0, Point3D p1)
        {
            SolidColorBrush brush = new SolidColorBrush(Colors.Black);
            var material = new DiffuseMaterial(brush);
            var mesh = new MeshGeometry3D();
            mesh.Positions.Add(p0);
            mesh.Positions.Add(p1);
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(0);
            GeometryModel3D geometryModel = new GeometryModel3D(mesh, material);
            model3DGroup.Children.Add(geometryModel);

            ModelVisual3D model = new ModelVisual3D
            {
                Content = model3DGroup
            };
            viewport.Children.Clear();
            viewport.Children.Add(model);
            viewport.Camera = PCamera;

            grid.Children.Clear();
            grid.Children.Add(viewport);
        }
        internal void DrawRectangle_Sample()
        {
            //ref. https://crynut84.tistory.com/75
            Rectangle r = new Rectangle();
            r.Width = 30;
            r.Height = 30;
            r.Margin = new Thickness(100, 100, 0, 0);
            r.Stroke = Brushes.Black;
            r.HorizontalAlignment = HorizontalAlignment.Left;
            r.VerticalAlignment = VerticalAlignment.Top;
            grid.Children.Add(r);
            //grid.Children.Remove(r);
        }

        /// <summary>
        /// Creates a ModelVisual3D containing a text label.
        /// </summary>
        /// <param name="text">The string</param>
        /// <param name="textColor">The color of the text.</param>
        /// <param name="bDoubleSided">Visible from both sides?</param>
        /// <param name="height">Height of the characters</param>
        /// <param name="center">The center of the label</param>
        /// <param name="over">Horizontal direction of the label</param>
        /// <param name="up">Vertical direction of the label</param>
        /// <returns>Suitable for adding to your Viewport3D</returns>
        public static GeometryModel3D CreateTextLabel3D(
            string text, Brush textColor, bool bDoubleSided, double height,
            Point3D center, Vector3D over, Vector3D up)
        {
            // First we need a textblock containing the text of our label
            TextBlock tb = new TextBlock(new Run(text));
            tb.Foreground = textColor;
            tb.FontFamily = new FontFamily("Arial");

            // Now use that TextBlock as the brush for a material
            DiffuseMaterial mat = new DiffuseMaterial();
            mat.Brush = new VisualBrush(tb);

            // We just assume the characters are square
            double width = text.Length * height;

            // Since the parameter coming in was the center of the label,
            // we need to find the four corners
            // p0 is the lower left corner
            // p1 is the upper left
            // p2 is the lower right
            // p3 is the upper right
            Point3D p0 = center - width / 2 * over - height / 2 * up;
            Point3D p1 = p0 + up * 1 * height;
            Point3D p2 = p0 + over * width;
            Point3D p3 = p0 + up * 1 * height + over * width;

            // Now build the geometry for the sign.  It's just a
            // rectangle made of two triangles, on each side.

            MeshGeometry3D mg = new MeshGeometry3D();
            mg.Positions = new Point3DCollection();
            mg.Positions.Add(p0);    // 0
            mg.Positions.Add(p1);    // 1
            mg.Positions.Add(p2);    // 2
            mg.Positions.Add(p3);    // 3

            if (bDoubleSided)
            {
                mg.Positions.Add(p0);    // 4
                mg.Positions.Add(p1);    // 5
                mg.Positions.Add(p2);    // 6
                mg.Positions.Add(p3);    // 7
            }

            mg.TriangleIndices.Add(0);
            mg.TriangleIndices.Add(3);
            mg.TriangleIndices.Add(1);
            mg.TriangleIndices.Add(0);
            mg.TriangleIndices.Add(2);
            mg.TriangleIndices.Add(3);

            if (bDoubleSided)
            {
                mg.TriangleIndices.Add(4);
                mg.TriangleIndices.Add(5);
                mg.TriangleIndices.Add(7);
                mg.TriangleIndices.Add(4);
                mg.TriangleIndices.Add(7);
                mg.TriangleIndices.Add(6);
            }

            // These texture coordinates basically stretch the
            // TextBox brush to cover the full side of the label.

            mg.TextureCoordinates.Add(new Point(0, 1));
            mg.TextureCoordinates.Add(new Point(0, 0));
            mg.TextureCoordinates.Add(new Point(1, 1));
            mg.TextureCoordinates.Add(new Point(1, 0));

            if (bDoubleSided)
            {
                mg.TextureCoordinates.Add(new Point(1, 1));
                mg.TextureCoordinates.Add(new Point(1, 0));
                mg.TextureCoordinates.Add(new Point(0, 1));
                mg.TextureCoordinates.Add(new Point(0, 0));
            }

            // And that's all.  Return the result.

            //ModelVisual3D mv3d = new ModelVisual3D();
            //mv3d.Content = new GeometryModel3D(mg, mat);
            //return mv3d;

            GeometryModel3D geoModel = new GeometryModel3D(mg, mat);
            return geoModel;
        }

        /// <summary>
        /// 사용자가 선택한 사각형 영역(p0, p1)에 해당하는 피라미드(좌표, 벡터 4개)를 반환합니다.
        /// 사용자가 그린 사각형의 내부 영역은 원근카메라 상태에서 피라미드 모양입니다.
        /// </summary>
        /// <param name="windowP0">사용자입력 사각형</param>
        /// <param name="windowP1">사용자입력 사각형</param>
        /// <param name="p0">계산된 피라미드</param>
        /// <param name="v0">계산된 피라미드</param>
        /// <param name="v1">계산된 피라미드</param>
        /// <param name="v2">계산된 피라미드</param>
        /// <param name="v3">계산된 피라미드</param>
        internal void GetInfinitePyramidBySelectionWindow(Point windowP0, Point windowP1,
            ref Point3D p0, ref Vector3D v0, ref Vector3D v1, ref Vector3D v2, ref Vector3D v3)
        {
            p0 = PCamera.Position;

            double wx1;
            double wx2;
            double wy1;
            double wy2;
            wx1 = Math.Min(windowP0.X, windowP1.X);
            wx2 = Math.Max(windowP0.X, windowP1.X);
            wy1 = Math.Min(windowP0.Y, windowP1.Y);
            wy2 = Math.Max(windowP0.Y, windowP1.Y);

            Point wP0 = new Point(wx1, wy1);
            Point wP1 = new Point(wx2, wy1);
            Point wP2 = new Point(wx2, wy2);
            Point wP3 = new Point(wx1, wy2);

            Point3D pyramidBottomP0 = GetPoint3d_FromPoint2D(wP0);
            Point3D pyramidBottomP1 = GetPoint3d_FromPoint2D(wP1);
            Point3D pyramidBottomP2 = GetPoint3d_FromPoint2D(wP2);
            Point3D pyramidBottomP3 = GetPoint3d_FromPoint2D(wP3);

            v0 = pyramidBottomP0 - p0;
            v1 = pyramidBottomP1 - p0;
            v2 = pyramidBottomP2 - p0;
            v3 = pyramidBottomP3 - p0;
        }
        /// <summary>
        /// 사용자가 그린 선분(p0, p1)에 해당하는 무한 부채꼴을 반환합니다.
        /// </summary>
        /// <param name="p0">사용자입력 선분</param>
        /// <param name="p1">사용자입력 선분</param>
        /// <param name="pos">계산된 부채꼴 중점</param>
        /// <param name="v0" >계산된 부채꼴</param>
        /// <param name="v2" >계산된 부채꼴</param>
        internal void GetInfiniteTriangleBySelectionFence(Point p0, Point p1,
            ref Point3D pos, ref Vector3D v0, ref Vector3D v1)
        {
            Point3D pp0 = GetPoint3d_FromPoint2D(p0);
            Point3D pp1 = GetPoint3d_FromPoint2D(p1);
            pos = PCamera.Position;
            v0 = pp0 - pos;
            v1 = pp1 - pos;
        }

    }
    public class PointMarker3D
    {
        readonly Draw3D instance;

        public ModelVisual3D modelVisual3D = new ModelVisual3D();

        internal bool visibility = false;
        double dia = 0.2;
        int resolution = 12;
        Shapes3D markerShapes = new Shapes3D();

        internal void Hide()
        {
            visibility = false;
            //markerShapes.Clear();
            modelVisual3D.Children.Clear();
        }
        internal void Show()
        {
            visibility = true;
        }

        Point3D position = new Point3D(0, 0, 0);
        Color color = Colors.Red;

        internal Point3D Position(Point point)
        {
            position = instance.GetPoint3dOnBasePlane_FromPoint2D(point);

            markerShapes.transform = new TranslateTransform3D(position.X, position.Y, position.Z);
            modelVisual3D.Content = markerShapes.Model3DGroup();
            return position;
        }

        public PointMarker3D(Draw3D instance)
        {
            this.instance = instance;
            markerShapes.AddSphere(position, dia, resolution);
            markerShapes.RecentShape.Color(color);
        }
    } //

    partial class Draw3D // Orbit & View
    {
        public PerspectiveCamera PCamera { get; set; } = new PerspectiveCamera
        {
            Position = new Point3D(-10, -10, 10),
            LookDirection = new Vector3D(10, 10, -10),
            FieldOfView = 70,
            FarPlaneDistance = 200,
            UpDirection = new Vector3D(0, 0, 1),
            NearPlaneDistance = 1
        };
        private PerspectiveCamera pCamera_init = new PerspectiveCamera();
        private OrthographicCamera oCamera = new OrthographicCamera
        {
            Position = new Point3D(-10, -10, 10),
            LookDirection = new Vector3D(10, 10, -10),
            FarPlaneDistance = 1000000000,
            UpDirection = new Vector3D(0, 0, 1),
            NearPlaneDistance = 0.1,
            Width = 20
        };

        private double preForcalDist;
        //회전등에 사용할 기준점 focalPoint 결정. shape.center 이용.
        private Point3D GetFocalPoint(PerspectiveCamera pCamera)
        {
            //shapes.center의 카메라 방향벡터 수직접점

            Point3D pos = pCamera.Position;
            Vector3D dir = pCamera.LookDirection;
            dir.Normalize();

            Point3D shapesCenter = shapes.Center();
            Vector3D cv = shapesCenter - pos;
            Point3D cvp = new Point3D(cv.X, cv.Y, cv.Z);
            double focalDist = GF.PlanePosition(dir, cvp);

            //if(double.IsNaN(focalDist)) focalDist = pCamera_init.FarPlaneDistance / 5; //가시거리의 1/5로 잡는게 좋을 것 같음.
            if (double.IsNaN(focalDist)) focalDist = preForcalDist;

            preForcalDist = focalDist;

            Point3D fp = pos + dir * focalDist;
            return fp;
        }

        //Free Orbit
        internal void OrbitStart()
        {
            pCamera_init = new PerspectiveCamera
            {
                //속성값들을 복사해주어야 깊은 복사가 됨. 복사를 안한 값들은 참조로 됨.
                Position = PCamera.Position,
                LookDirection = PCamera.LookDirection,
                UpDirection = PCamera.UpDirection,
                FieldOfView = PCamera.FieldOfView,
                FarPlaneDistance = PCamera.FarPlaneDistance,
                NearPlaneDistance = PCamera.NearPlaneDistance
            };
        }
        internal void OrbitMove(Vector mov)
        {
            //Point3D pos = pCamera_init.Position;
            //Vector3D dir = pCamera_init.LookDirection;
            //Vector3D up = pCamera_init.UpDirection;
            //
            //Vector3D rightVector = Vector3D.CrossProduct(dir, up);
            //Vector3D upVector = Vector3D.CrossProduct(rightVector, dir);
            //rightVector.Normalize();
            //upVector.Normalize();

            Point3D pos = pCamera_init.Position;
            Vector3D dir = pCamera_init.LookDirection;
            Vector3D up = pCamera_init.UpDirection;
            dir.Normalize();
            up.Normalize();
            Vector3D camY = Vector3D.CrossProduct(up, dir);
            up = Vector3D.CrossProduct(dir, camY);
            up.Normalize();
            camY.Normalize();

            double width = grid.ActualWidth;
            //double height = grid.ActualHeight;

            double fovW = PCamera.FieldOfView;
            //double fovH = Math.Atan(Math.Tan(fovW / 2 / 180 * Math.PI) / width * height) * 2 * 180 / Math.PI;


            //마우스로 pan을 하려면 어차피 카메라와의 특정거리를 선정해야함
            //Shapes의 center 속성으로부터 특정 거리 선정함
            //그 특정거리에 있는 점을 마우스로 잡고 움직이는 것으로 가정.
            Point3D shapesCenter = shapes.Center();
            Vector3D cv = shapesCenter - pos;
            Point3D cvp = cv + new Point3D(0, 0, 0);
            double targetDist = GF.PlanePosition(dir, cvp);

            double width3d = Math.Tan(fovW / 2 / 180 * Math.PI) * targetDist;
            double resolution = width3d / (width / 2);

            Vector3D pv = new Vector3D(pos.X, pos.Y, pos.Z);
            Vector3D np = pv + camY * mov.X * resolution + up * mov.Y * resolution;

            PCamera.Position = new Point3D(np.X, np.Y, np.Z);
        }
        internal void OrbitRotate(Vector mov)
        {
            Point3D pos = pCamera_init.Position;
            Vector3D dir = pCamera_init.LookDirection;
            Vector3D up = pCamera_init.UpDirection;
            dir.Normalize();
            up.Normalize();
            Vector3D camY = Vector3D.CrossProduct(up, dir);
            up = Vector3D.CrossProduct(dir, camY);
            up.Normalize();

            Point3D fp = GetFocalPoint(pCamera_init);
            Vector3D distVector = fp - pos;
            double focalDist = distVector.Length;

            Vector3D cv = pos - fp; //초점에 대한 카메라 위치 백터

            Vector3D localXVector = Vector3D.CrossProduct(dir, up);
            Vector3D localYVector = up;
            Vector3D localZVector = Vector3D.CrossProduct(localXVector, dir);
            localXVector.Normalize();
            localYVector.Normalize();
            localZVector.Normalize();

            Vector3D moveVector = -mov.X * localXVector + mov.Y * localZVector;
            double moveLength = moveVector.Length;
            //double moveRad = moveLength / width * 4;
            double moveRad = moveLength / 150; //화면이 커지면 더 많이 돌아가게 하는방식이 더 좋음.
            Vector3D moveDir = moveVector;
            moveDir.Normalize();

            Vector3D ncv = Math.Cos(moveRad) * cv + Math.Sin(moveRad) * focalDist * moveDir;

            Point3D newPos = fp + ncv;
            PCamera.Position = newPos;

            //카메라 방향
            Vector3D newDir = fp - newPos; //새 카메라 방향
            PCamera.LookDirection = newDir;

            Vector3D newUp = Vector3D.CrossProduct(ncv * Math.Abs(mov.Y) + cv * Math.Abs(mov.X), localXVector);
            PCamera.UpDirection = newUp;
        }
        internal void OrbitTwist(double rad)
        {
            Vector3D u = pCamera_init.UpDirection;
            Vector3D d = pCamera_init.LookDirection + new Vector3D(0, 0, 0);
            d.Normalize();
            Vector3D nu = GF.RotateVector3D(u, new Point3D(0, 0, 0), d, rad);

            PCamera.UpDirection = nu;
        }
        internal void OrbitTwist_Old(double rad, double dist)
        {
            Point3D p = pCamera_init.Position + new Vector3D(0, 0, 0);
            Vector3D pv = new Vector3D(p.X, p.Y, p.Z);  //카메라 위치
            Vector3D u = pCamera_init.UpDirection;
            Vector3D d = pCamera_init.LookDirection + new Vector3D(0, 0, 0);
            d.Normalize();
            Vector3D nu = GF.RotateVector3D(u, new Point3D(0, 0, 0), d, rad);

            PCamera.LookDirection = d;
            PCamera.Position = p;
            PCamera.UpDirection = nu;
        }
        internal void OrbitEnd()
        {
            //iniPCamera = myPCamera;
        }

        //Zoom
        internal void ZoomForward(int delta)
        {
            Point3D p = PCamera.Position;
            Vector3D pv = new Vector3D
            {
                X = p.X,
                Y = p.Y,
                Z = p.Z
            };
            Vector3D fp = pv + PCamera.LookDirection; //초점
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

            Vector3D np = fp + cv * newDist / dist;

            p.X = np.X;
            p.Y = np.Y;
            p.Z = np.Z;
            PCamera.Position = p;
        }
        internal void ViewZoomPoints(List<Point3D> points)
        {
            double initialCameraDistance = 20;

            Point3D pos = PCamera.Position;
            Vector3D dir = PCamera.LookDirection;
            Vector3D up = PCamera.UpDirection;
            dir.Normalize();
            up.Normalize();
            Vector3D camY = Vector3D.CrossProduct(up, dir);
            up = Vector3D.CrossProduct(dir, camY);
            up.Normalize();
            camY.Normalize();

            double width = grid.ActualWidth;
            double height = grid.ActualHeight;

            double fovW = PCamera.FieldOfView;
            double fovH = Math.Atan(Math.Tan(fovW / 2 / 180 * Math.PI) / width * height) * 2 * 180 / Math.PI;

            if (points.Count == 0)
            {
                pos = new Point3D(10, 0, 0) - dir * initialCameraDistance;
                PCamera.Position = pos;
                return;
            }

            if (points.Count == 1)
            {
                pos = points[0] - dir * initialCameraDistance;
                PCamera.Position = pos;
                return;
            }

            Vector3D lPlane = GF.RotationVector3D(camY, up, fovW / 2);
            Vector3D rPlane = GF.RotationVector3D(-camY, up, -fovW / 2);
            Vector3D uPlane = GF.RotationVector3D(up, camY, -fovH / 2);
            Vector3D bPlane = GF.RotationVector3D(-up, camY, fovH / 2);

            double lPosition = GF.PlanePosition(lPlane, points[0]);
            double rPosition = GF.PlanePosition(rPlane, points[0]);
            double uPosition = GF.PlanePosition(uPlane, points[0]);
            double bPosition = GF.PlanePosition(bPlane, points[0]);

            foreach (Point3D point in points)
            {
                lPosition = Math.Max(GF.PlanePosition(lPlane, point), lPosition);
                rPosition = Math.Max(GF.PlanePosition(rPlane, point), rPosition);
                uPosition = Math.Max(GF.PlanePosition(uPlane, point), uPosition);
                bPosition = Math.Max(GF.PlanePosition(bPlane, point), bPosition);
            }

            Point3D lPoint = new Point3D(0, 0, 0) + lPlane * lPosition;
            Point3D rPoint = new Point3D(0, 0, 0) + rPlane * rPosition;
            Point3D uPoint = new Point3D(0, 0, 0) + uPlane * uPosition;
            Point3D bPoint = new Point3D(0, 0, 0) + bPlane * bPosition;

            Point3D p1 = GF.CrossPoint_3Planes(uPlane, uPoint, bPlane, bPoint, lPlane, lPoint);
            Point3D p2 = GF.CrossPoint_3Planes(uPlane, uPoint, bPlane, bPoint, rPlane, rPoint);
            Point3D p3 = GF.CrossPoint_3Planes(lPlane, lPoint, rPlane, rPoint, uPlane, uPoint);
            Point3D p4 = GF.CrossPoint_3Planes(lPlane, lPoint, rPlane, rPoint, bPlane, bPoint);

            Point3D cp12 = new Point3D((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2, (p1.Z + p2.Z) / 2);
            Point3D cp34 = new Point3D((p3.X + p4.X) / 2, (p3.Y + p4.Y) / 2, (p3.Z + p4.Z) / 2);
            double cp12pos = GF.PlanePosition(dir, cp12);
            double cp34pos = GF.PlanePosition(dir, cp34);

            Point3D newPos;
            if (cp12pos > cp34pos)
            {
                newPos = cp34;
            }
            else
            {
                newPos = cp12;
            }

            PCamera.Position = newPos;

        }
        internal void ViewZoomExtend()
        {
            List<Point3D> points = new List<Point3D>();
            foreach (Shape3D shape in shapes)
            {
                points.Add(shape.BasePoint);
            }
            ViewZoomPoints(points);
        }
        internal void ViewZoomWindow(Point inpP0, Point inpP1)
        {
            Point3D p0 = GetPoint3dOnBasePlane_FromPoint2D(inpP0);
            Point3D p1 = GetPoint3dOnBasePlane_FromPoint2D(inpP1);
            ViewZoomWindow(p0, p1);
        }
        internal void ViewZoomWindow(Point3D p0, Point3D p1)
        {
            List<Point3D> points = new List<Point3D>();
            points.Add(p0);
            points.Add(p1);

            ViewZoomPoints(points);
        }

        private bool isOnZoomPan_WheelScroll = false;
        private Point pointMouseDown;
        public bool IsOnZoomPan_WheelScroll
        {
            get
            {
                return isOnZoomPan_WheelScroll;
            }
            set
            {
                if (value)
                {
                    grid.MouseWheel += new MouseWheelEventHandler(Zoom_MouseWheelScroll);
                    grid.MouseDown += PanOn_MouseWheelDown;
                    grid.MouseUp += PanOff_MouseWheelUp;
                }
                else
                {
                    grid.MouseWheel -= new MouseWheelEventHandler(Zoom_MouseWheelScroll);
                    grid.MouseDown -= PanOn_MouseWheelDown;
                    grid.MouseUp -= PanOff_MouseWheelUp;
                    grid.MouseMove -= Pan_MouseWheelDownMove;
                }
                isOnZoomPan_WheelScroll = value;
            }
        }
        private void Zoom_MouseWheelScroll(object sender, MouseWheelEventArgs e)
        {
            ZoomForward(e.Delta);
        }
        private void PanOn_MouseWheelDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                pointMouseDown = e.GetPosition(grid);
                OrbitStart();
                grid.MouseMove += Pan_MouseWheelDownMove;
            }
        }
        private void Pan_MouseWheelDownMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                Point p = e.GetPosition(grid);
                Vector mov = p - pointMouseDown;
                //bckD.OrbitMoveX(mov.X / 2); //MoveX와 MoveY중 처음 실행된 것 하나만 동작함.
                //bckD.OrbitMoveY(mov.Y / 2);
                OrbitMove(mov);
            }
        }
        private void PanOff_MouseWheelUp(object sender, MouseButtonEventArgs e)
        {
            OrbitEnd();
            grid.MouseMove -= Pan_MouseWheelDownMove;
        }


        public void ViewTop()
        {
            Point3D focalPoint = GetFocalPoint(PCamera);
            Vector3D distVector = focalPoint - PCamera.Position;
            Point3D newPos = focalPoint;
            newPos.Z += distVector.Length;

            PCamera.Position = newPos;
            PCamera.LookDirection = focalPoint - newPos;
            PCamera.UpDirection = new Vector3D(0, 1, 0);
        }
        public void ViewFront()
        {
            Point3D focalPoint = GetFocalPoint(PCamera);
            Vector3D distVector = focalPoint - PCamera.Position;
            Point3D newPos = focalPoint;
            newPos.Y -= distVector.Length;

            PCamera.Position = newPos;
            PCamera.LookDirection = focalPoint - newPos;
            PCamera.UpDirection = new Vector3D(0, 0, 1);
        }
        public void ViewBack()
        {
            Point3D focalPoint = GetFocalPoint(PCamera);
            Vector3D distVector = focalPoint - PCamera.Position;
            Point3D newPos = focalPoint;
            newPos.Y += distVector.Length;

            PCamera.Position = newPos;
            PCamera.LookDirection = focalPoint - newPos;
            PCamera.UpDirection = new Vector3D(0, 0, 1);
        }
        public void ViewRight()
        {
            Point3D focalPoint = GetFocalPoint(PCamera);
            Vector3D distVector = focalPoint - PCamera.Position;
            Point3D newPos = focalPoint;
            newPos.X += distVector.Length;

            PCamera.Position = newPos;
            PCamera.LookDirection = focalPoint - newPos;
            PCamera.UpDirection = new Vector3D(0, 0, 1);
        }
        public void ViewLeft()
        {
            Point3D focalPoint = GetFocalPoint(PCamera);
            Vector3D distVector = focalPoint - PCamera.Position;
            Point3D newPos = focalPoint;
            newPos.X -= distVector.Length;

            PCamera.Position = newPos;
            PCamera.LookDirection = focalPoint - newPos;
            PCamera.UpDirection = new Vector3D(0, 0, 1);
        }
        public void ViewBottom()
        {
            Point3D focalPoint = GetFocalPoint(PCamera);
            Vector3D distVector = focalPoint - PCamera.Position;
            Point3D newPos = focalPoint;
            newPos.Z -= distVector.Length;

            PCamera.Position = newPos;
            PCamera.LookDirection = focalPoint - newPos;
            PCamera.UpDirection = new Vector3D(0, 1, 0);
        }
        public void ViewSE()
        {
            Point3D focalPoint = GetFocalPoint(PCamera);
            Vector3D distVector = focalPoint - PCamera.Position;
            Point3D newPos = focalPoint;
            double dist = distVector.Length;
            double distZ = dist * Math.Sin(Math.PI / 4);
            double distXY = distZ * Math.Sin(Math.PI / 4);
            newPos.X += distXY;
            newPos.Y -= distXY;
            newPos.Z += distZ;

            PCamera.Position = newPos;
            PCamera.LookDirection = focalPoint - newPos;
            PCamera.UpDirection = new Vector3D(0, 0, 1);
        }
        public void ViewSW()
        {
            Point3D focalPoint = GetFocalPoint(PCamera);
            Vector3D distVector = focalPoint - PCamera.Position;
            Point3D newPos = focalPoint;
            double dist = distVector.Length;
            double distZ = dist * Math.Sin(Math.PI / 4);
            double distXY = distZ * Math.Sin(Math.PI / 4);
            newPos.X -= distXY;
            newPos.Y -= distXY;
            newPos.Z += distZ;

            PCamera.Position = newPos;
            PCamera.LookDirection = focalPoint - newPos;
            PCamera.UpDirection = new Vector3D(0, 0, 1);
        }
        public void ViewNW()
        {
            Point3D focalPoint = GetFocalPoint(PCamera);
            Vector3D distVector = focalPoint - PCamera.Position;
            Point3D newPos = focalPoint;
            double dist = distVector.Length;
            double distZ = dist * Math.Sin(Math.PI / 4);
            double distXY = distZ * Math.Sin(Math.PI / 4);
            newPos.X -= distXY;
            newPos.Y += distXY;
            newPos.Z += distZ;

            PCamera.Position = newPos;
            PCamera.LookDirection = focalPoint - newPos;
            PCamera.UpDirection = new Vector3D(0, 0, 1);
        }
        public void ViewNE()
        {
            Point3D focalPoint = GetFocalPoint(PCamera);
            Vector3D distVector = focalPoint - PCamera.Position;
            Point3D newPos = focalPoint;
            double dist = distVector.Length;
            double distZ = dist * Math.Sin(Math.PI / 4);
            double distXY = distZ * Math.Sin(Math.PI / 4);
            newPos.X += distXY;
            newPos.Y += distXY;
            newPos.Z += distZ;

            PCamera.Position = newPos;
            PCamera.LookDirection = focalPoint - newPos;
            PCamera.UpDirection = new Vector3D(0, 0, 1);
        }

        private void MotionMoveCamera(PerspectiveCamera newCamera)
        {
            int numFrame = 100;
            int dwStartTime = System.Environment.TickCount;

            Point3D sp = PCamera.Position;
            Point3D ep = newCamera.Position;
            Vector3D dp = new Vector3D(ep.X - sp.X, ep.Y - sp.Y, ep.Z - sp.Z);
            dp /= numFrame;

            Vector3D sl = PCamera.LookDirection;
            Vector3D el = newCamera.LookDirection;
            Vector3D dl = new Vector3D(el.X - sl.X, el.Y - sl.Y, el.Z - sl.Z);
            dl /= numFrame;

            Vector3D su = PCamera.UpDirection;
            Vector3D eu = newCamera.UpDirection;
            Vector3D du = new Vector3D(eu.X - su.X, eu.Y - su.Y, eu.Z - su.Z);
            du /= numFrame;


            for (int i = 0; i <= numFrame; i++)
            {
                PCamera.Position = new Point3D(sp.X + dp.X * i, sp.Y + dp.Y * i, sp.Z + dp.Z * i);
                PCamera.LookDirection = new Vector3D(sl.X + dl.X * i, sl.Y + dl.Y * i, sl.Z + dl.Z * i);
                PCamera.UpDirection = new Vector3D(su.X + du.X * i, su.Y + du.Y * i, su.Z + du.Z * i);

                while (true)
                {
                    if (System.Environment.TickCount - dwStartTime > 1000) break; //1000 milliseconds 
                }
                viewport.UpdateLayout();

            }

            //myPCamera.Position = ep;
            //myPCamera.LookDirection = el;
            //myPCamera.UpDirection = eu;

        }
    }

    public class Shapes3D : List<Shape3D>
    {
        public Model3DGroup modelGroup = new Model3DGroup();
        private Shape3D recentShape;

        public TranslateTransform3D transform;

        internal Shape3D RecentShape { get => recentShape; set => recentShape = value; }

        public Shapes3D()
        {

        }

        private new Shape3D Add(Shape3D shape)
        {
            base.Add(shape);
            RecentShape = shape;
            return shape;
        }

        internal Triangle3D AddTriangle(Point3D p0, Point3D p1, Point3D p2)
        {
            Triangle3D t = new Triangle3D(p0, p1, p2);
            Add(t);
            return t;
        }
        internal Rectangular3D AddRectangle(Point3D p0, Point3D p1, Point3D p2, Point3D p3)
        {
            Rectangular3D t = new Rectangular3D(p0, p1, p3, p2);
            Add(t);
            return t;
        }
        public Line3D AddLine(Point3D sp, Point3D ep)
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
        internal Circle3D AddCircle(double radius, Vector3D normal, Point3D center, int resolution)
        {
            Circle3D c = new Circle3D(radius, normal, center, resolution);
            Add(c);
            return c;
        }
        internal Cone3D AddCone(double radius, Vector3D heightVector, Point3D center, int resolution)
        {
            Cone3D c = new Cone3D(radius, heightVector, center, resolution);
            Add(c);
            return c;
        }
        internal Cylinder3D AddCylinder(Point3D str, Vector3D dir, double dia, int resolution)
        {
            Cylinder3D c = new Cylinder3D(str, dir, dia, resolution);
            Add(c);
            return c;
        }
        internal Cylinder3D AddCylinderClosed(Point3D str, Vector3D dir, double dia, int resolution)
        {
            Cylinder3D s = new Cylinder3D(str, dir, dia, resolution);
            s.Close();
            Add(s);
            return s;
        }
        internal Arrow3D AddArrow(Point3D str, Vector3D dir, double dia, int resolution)
        {
            Arrow3D s = new Arrow3D(str, dir, dia, resolution);
            Add(s);
            return s;
        }
        internal Arrow3D AddForce(Point3D target, Vector3D force)
        {
            double dia = force.Length / 10;
            int resolution = 12;
            Arrow3D s = new Arrow3D(target - force, force, dia, resolution);
            Add(s);
            return s;
        }
        public Sphere3D AddSphere(Point3D point, double diameter, int resolution)
        {
            Sphere3D s = new Sphere3D(point, diameter, resolution);
            Add(s);
            return s;
        }

        internal Polygon3D AddPolygon(Point3D str, Vector3D dir, SectionPoly poly)
        {
            Polygon3D s = new Polygon3D(str, dir, poly);
            Add(s);
            return s;

        }

        internal Model3DGroup Model3DGroup()
        {
            Model3DGroup model3DGroup = new Model3DGroup();
            foreach (Shape3D shape in this)
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
            foreach (Shape3D shape in this)
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
    public class Shape3D
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
        public Shape3D()
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
    class Triangle3D : Shape3D
    {
        private Point3D p0;
        private Point3D p1;
        private Point3D p2;
        public Triangle3D(Point3D p0, Point3D p1, Point3D p2)
        {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;

            mesh = MeshGenerator.Triangle(p0, p1, p2);
        }
    }
    class Rectangular3D : Shape3D
    {
        private Point3D p0;
        private Point3D p1;
        private Point3D p2;
        private Point3D p3;
        public Rectangular3D(Point3D p0, Point3D p1, Point3D p2, Point3D p3)
        {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;

            mesh = MeshGenerator.Rectangle(p0, p1, p2, p3);
        }
    }
    public class Line3D : Shape3D
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
    class Hexahedron : Shape3D
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
    class Cone3D : Shape3D
    {
        private double radius;
        private Vector3D heightVector;
        private Point3D center;
        private int resolution;
        double height;

        public Cone3D(double radius, Vector3D heightVector, Point3D center, int resolution)
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
    class Circle3D : Shape3D
    {
        private double radius;
        private Vector3D normal;
        private Point3D center;
        private int resolution;

        public Circle3D(double radius, Vector3D normal, Point3D center, int resolution)
        {
            this.radius = radius;
            this.normal = normal;
            this.center = center;
            this.resolution = resolution;

            mesh = MeshGenerator.Circle(radius, resolution);

            SetTransforms(center, normal);
        }
    }
    class Cylinder3D : Shape3D
    {
        private Point3D str;
        private Vector3D dir;
        private double dia;
        private int resolution;
        private bool closed;

        public Cylinder3D(Point3D str, Vector3D dir, double dia, int resolution)
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
    class Polygon3D : Shape3D
    {
        private Point3D str;
        private Vector3D dir;
        private SectionPoly poly;

        public Polygon3D(Point3D str, Vector3D dir, SectionPoly poly)
        {
            this.str = str;
            this.dir = dir;
            this.poly = poly;

            mesh = MeshGenerator.Polygon(poly, dir.Length);
            Vector3D up = new Vector3D(1, 0, 0);
            SetTransforms(str, dir);
        }
    }
    class Arrow3D : Shape3D
    {
        private Point3D str;
        private Vector3D dir;
        private double dia;
        private int resolution;
        private bool closed;

        public Arrow3D(Point3D str, Vector3D dir, double dia, int resolution)
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
    public class Sphere3D : Shape3D
    {
        private Point3D center;
        private double diameter;
        private int resolution;

        public Sphere3D(Point3D center, double diameter, int resolution)
        {
            this.center = center;
            this.diameter = diameter;
            this.resolution = resolution;

            mesh = MeshGenerator.Sphere(diameter, resolution);
            SetTransforms(center, new Vector3D(1, 1, 1));
        }
    }
    public class TextShapes3D : List<TextShape3D>
    {
        public TextShape3D Add(string caption, Point3D position, double size)
        {
            TextShape3D t = new TextShape3D(caption, position, size, Colors.Black);
            base.Add(t);
            return t;
        }
        internal Model3DGroup Model3DGroup()
        {
            Model3DGroup model3DGroup = new Model3DGroup();
            foreach (TextShape3D text in this)
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
    public class TextShape3D : Shape3D
    {
        string caption;
        Point3D position;
        double size = 8;
        Color color;

        public TextShape3D(string caption, Point3D position, double size, Color color)
        {
            this.caption = caption;
            this.position = position;
            this.size = size;
            this.color = color;
        }

        internal new GeometryModel3D GeoModel()
        {
            GeometryModel3D geo3D;
            geo3D = Draw3D.CreateTextLabel3D(caption, Brushes.Red, true, 1, position,
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
        internal static MeshGeometry3D TriangleOneSide(Point3D p0, Point3D p1, Point3D p2)
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
        internal static MeshGeometry3D Triangle(Point3D p0, Point3D p1, Point3D p2)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.Positions.Add(p0);
            mesh.Positions.Add(p1);
            mesh.Positions.Add(p2);

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(1);

            //맞나 확인 필요 200505
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);

            Vector3D normal = new Vector3D(0, 0, 1);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
            return mesh;
        }
        internal static MeshGeometry3D Rectangle(Point3D p0, Point3D p1, Point3D p2, Point3D p3)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.Positions.Add(p0);
            mesh.Positions.Add(p1);
            mesh.Positions.Add(p2);
            mesh.Positions.Add(p3);

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);

            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(3);

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(1);

            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(2);

            Vector3D normal = new Vector3D(0, 0, 1);
            mesh.Normals.Add(normal);
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
                angle = Math.PI / 2 - Math.PI / 3 * i;
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
        private readonly Canvas grid;
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


        public SelectionWindow(Canvas grid)
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

