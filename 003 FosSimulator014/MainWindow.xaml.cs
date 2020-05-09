using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _003_FosSimulator014
{
    public partial class MainWindow : Window
    {
        private readonly SMR smr;
        public readonly FEM fem;
        public readonly DRAW draw;
        public readonly CommandWindow cmd;
        internal RequestUserInput requestUserInput;

        public MainWindow()
        {
            InitializeComponent();

            dkpStructureConcrete.Visibility = Visibility.Collapsed;
            dkpCameraControl.Visibility = Visibility.Collapsed;
            dkpFemWorks.Visibility = Visibility.Collapsed;

            smr = new SMR();
            fem = new FEM();
            draw = new DRAW(grdMain);
            cmd = new CommandWindow(this,tbxCommand);
            requestUserInput = new RequestUserInput(this);

            TurnOnWheelPanZoom();
            WindowSelectionOn(true);
            TurnOnDeselectAll_Esc(true);
            TurnOnErase_Del(true);

            draw.ViewTop();
            draw.ViewZoomExtend();

            //TestNodeGrid();
            //TestExtrude();
            TestDivide();

            RedrawFemModel();
        }

        private void TestDivide()
        {
            Node n1 = fem.model.nodes.Add(0, 0, 0);
            Node n2 = fem.model.nodes.Add(10, 0, 0);
            fem.model.elems.AddFrame(n1, n2);

            cmd.Call("Erase");
            cmd.Call("All");
            cmd.Call("Line");
            cmd.Call("0,0");
            cmd.Call("10,0");
            cmd.Call(" ");
            cmd.Call("Select");
            cmd.Call("Element");
            cmd.Call("1");
            cmd.Call("Extrude");
            cmd.Call("@0,1");
            cmd.Call("5");

        }
        private void TestExtrude()
        {
            fem.Initialize();
            MaterialFem matl1 = fem.model.materials.AddConcrete("C30");
            Section sect1 = fem.model.sections.AddRectangle(1, 0.2);

            Node n1 = fem.model.nodes.Add(0, 0, 0);
            Node n2 = fem.model.nodes.Add(10, 0, 0);
            Frame f = fem.model.elems.AddFrame(n1, n2);
            f.material = matl1;
            f.Section(sect1);
            fem.Select(f);
            fem.Divide(2);

            fem.SelectElemAll();
            fem.Divide(2);

            fem.SelectElemAll();
            fem.Divide(2);

            fem.SelectElemAll();
            Vector3D dir = new Vector3D(0, 1, 0);
            fem.Extrude(dir, 5);

            fem.SelectElemAll();
            dir = new Vector3D(0, 0, 1);
            fem.Extrude(dir, 5);

        }
        private void TestNodeGrid()
        {
            for(int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    Point3D p0 = new Point3D(i, j, 0);

                    fem.model.nodes.Add(i, j, 0);
                }
            }
        }

        private void StartAddNode(object sender, RoutedEventArgs e)
        {
            WindowSelectionOn(false);
            MouseMove += ShowPointMarker_MouseMove;
            MouseDown += AddNode_MouseLeftDown;
            KeyUp += EndAddNode_Esc;
            draw.RedrawShapes();
        }
        private void ShowPointMarker_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            draw.pointMarker.visibility = true;
            Point p = e.GetPosition(grdMain);
            Point3D p3d = draw.pointMarker.Position(p);
            stbLabel.Content = "Add Node at (" + p3d.X + ", " + p3d.Y + ", " + p3d.Z + ")";
            draw.RedrawShapes();
        }
        private void AddNode_MouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton==MouseButtonState.Pressed)
            {
                Point3D p3d = draw.GetPoint3dOnBasePlane_FromPoint2D(e.GetPosition(grdMain));
                fem.model.nodes.Add(p3d);
                RedrawFemModel();
            }
        }
        private void EndAddNode_Esc(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                draw.pointMarker.Hide();
                MouseMove -= ShowPointMarker_MouseMove;
                MouseDown -= AddNode_MouseLeftDown;
                KeyUp -= EndAddNode_Esc;
                draw.RedrawShapes();
                WindowSelectionOn(true);
            }
        }

        internal void SelectElem()
        {
            requestUserInput = new RequestUserInput(this);
            requestUserInput.RequestInts(R.SelectElementNumber);
            requestUserInput.actionAfterIntsWithInts += fem.SelectElem;
            requestUserInput.Start();
        }
        internal void SelectNode()
        {
            requestUserInput = new RequestUserInput(this);
            requestUserInput.RequestInts(R.String2);
            requestUserInput.actionAfterIntsWithInts += fem.SelectNode;
            requestUserInput.Start();
        }

        private void FemTest_Solid003(object sender, RoutedEventArgs e)
        {
            fem.Initialize();
            MaterialFem matl1 = fem.model.materials.AddConcrete("C30");
            Section sect1 = fem.model.sections.AddRectangle(1, 0.2);

            Node n1 = fem.model.nodes.Add(0, 0, 0);
            Node n2 = fem.model.nodes.Add(2, 0, 0);
            Frame f = fem.model.elems.AddFrame(n1, n2);
            f.material = matl1;
            f.Section(sect1);
            fem.Select(f);
            fem.Divide(2);

            fem.SelectElemAll();
            Vector3D dir = new Vector3D(0, 1, 0);
            Elements ee = fem.Extrude(dir, 2);
            
            dir = new Vector3D(0, 0, 0.5);
            fem.Extrude(ee, dir, 20);
            
            fem.model.boundaries.AddBoundary(fem.model.nodes[0], 1, 1, 1, 1, 1, 1);
            fem.model.boundaries.AddBoundary(fem.model.nodes[1], 1, 1, 1, 1, 1, 1);
            fem.model.boundaries.AddBoundary(fem.model.nodes[2], 1, 1, 1, 1, 1, 1);
            fem.model.boundaries.AddBoundary(fem.model.nodes[3], 1, 1, 1, 1, 1, 1);
            fem.model.boundaries.AddBoundary(fem.model.nodes[4], 1, 1, 1, 1, 1, 1);
            fem.model.boundaries.AddBoundary(fem.model.nodes[5], 1, 1, 1, 1, 1, 1);
            fem.model.boundaries.AddBoundary(fem.model.nodes[6], 1, 1, 1, 1, 1, 1);
            fem.model.boundaries.AddBoundary(fem.model.nodes[7], 1, 1, 1, 1, 1, 1);
            fem.model.boundaries.AddBoundary(fem.model.nodes[8], 1, 1, 1, 1, 1, 1);
            
            Vector3D force = new Vector3D(1000, 0, 0);
            Vector3D moment = new Vector3D(0, 0, 0);
            
            Node np = fem.model.nodes.GetNode(188);
            if (np != null)
            {
                fem.loads.AddNodal(np, force, moment);
            }


            //double[,] mat = new double[3, 3];
            //mat[0, 0] = 1; mat[0, 1] = 2; mat[0, 2] = 2;
            //mat[1, 0] = 1; mat[1, 1] = 2; mat[1, 2] = 3;
            //mat[2, 0] = 2; mat[2, 1] = 2; mat[2, 2] = 3;
            ////double[,] mat = new double[2, 2];
            ////mat[0, 0] = 1; mat[0, 1] = 2;
            ////mat[1, 0] = 1; mat[1, 1] = 1;
            //double det = GF.Determinant(mat);


            fem.Solve();
            RedrawFemModel();
        }
        private void FemTest_SimpleBeamLoadZ(object sender, RoutedEventArgs e)
        {
            fem.Initialize();
            MaterialFem matl1 = fem.model.materials.AddConcrete("C30");

            double width = 1;
            double height = 0.2;
            Section sect1 = fem.model.sections.AddRectangle(width, height);

            Node n1 = fem.model.nodes.Add(0, 0, 0);
            Node n2 = fem.model.nodes.Add(10, 0, 0);
            Frame f = fem.model.elems.AddFrame(n1, n2);
            f.material = matl1;
            f.Section(sect1);
            
            fem.Select(f);
            fem.Divide(10);

            fem.model.boundaries.AddBoundary(n1, 1, 1, 1, 1, 0, 0);
            fem.model.boundaries.AddBoundary(n2, 0, 1, 1, 0, 0, 0);

            Vector3D force = new Vector3D(0, 0, -50);
            Vector3D moment = new Vector3D(0, 0, 0);

            Node np = fem.model.nodes.GetNode(4);
            fem.loads.AddNodal(np, force, moment);

            fem.Solve();

            RedrawFemModel();
        }
        private void FemTest_SimpleBeamLoadY(object sender, RoutedEventArgs e)
        {
            fem.Initialize();
            MaterialFem matl1 = fem.model.materials.AddConcrete("C30");

            double width = 1;
            double height = 0.2;
            Section sect1 = fem.model.sections.AddRectangle(width, height);

            Node n1 = fem.model.nodes.Add(0, 0, 0);
            Node n2 = fem.model.nodes.Add(10, 0, 0);
            Frame f = fem.model.elems.AddFrame(n1, n2);
            f.material = matl1;
            f.Section(sect1);

            fem.Select(f);
            fem.Divide(10);

            fem.model.boundaries.AddBoundary(n1, 1, 1, 1, 1, 0, 0);
            fem.model.boundaries.AddBoundary(n2, 0, 1, 1, 0, 0, 0);

            Vector3D force = new Vector3D(0, 600, 0);
            Vector3D moment = new Vector3D(0, 0, 0);

            Node np = fem.model.nodes.GetNode(7);
            fem.loads.AddNodal(np, force, moment);

            fem.Solve();

            RedrawFemModel();
        }

        private void ExitApplication(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DrawSampleGradient(object sender, RoutedEventArgs e)
        {
            draw.DrawGradient_Sample();
        }
        private void DrawCone(object sender, RoutedEventArgs e)
        {
            double radius = 5;
            Point3D center = new Point3D(-1, -1, -1);
            Vector3D heightVector = new Vector3D(10,0, 0);
            //bckD.DrawCone(center, radius, heightVector, resolution, Colors.AliceBlue);
            draw.shapes.AddCone(radius, heightVector, center, 6);
            draw.RedrawShapes();
        }
        private void DrawCoordinationMark(object sender, RoutedEventArgs e)
        {
            //Draw 좌표계
            double len = 4;
            double dia = 0.5;
            int resolution = 8;
            Point3D str = new Point3D(0, 0, 0);
            Vector3D dir = new Vector3D(len, 0, 0);
            
            //bckD.DrawCylinderClosed(str, dir, dia, resolution, Colors.Red);
            draw.shapes.AddCylinderClosed(str, dir, dia, resolution);
            draw.shapes.RecentShape.Color(Colors.Red);


            dir = new Vector3D(0, len, 0);
            draw.shapes.AddCylinderClosed(str, dir, dia, resolution);
            draw.shapes.RecentShape.Color(Colors.Green);

            dir = new Vector3D(0, 0, len);
            //bckD.DrawCylinderClosed(str, dir, dia, resolution, Colors.Black);
            draw.shapes.AddCylinderClosed(str, dir, dia, resolution);
            draw.shapes.RecentShape.Color(Colors.Black);
            //bckD.shapes.AddBox(new Point3D(0, 0, 0), new Vector3D(10, 10, 10));
            draw.RedrawShapes();
        }
        private void DrawSphere(object sender, RoutedEventArgs e)
        {
            Point3D point = new Point3D(0, 0, 0);
            double diameter = 5;
            int resolution = 48;

            draw.shapes.AddSphere(point, diameter, resolution);
            draw.shapes.RecentShape.Color(Colors.Red);
            draw.RedrawShapes();
        }
        private void DrawPerformanceTest(object sender, RoutedEventArgs e)
        {
            for (int i = 1; i < 20; i++)
            {
                for (int j = 1; j < 20; j++)
                {
                    for (int k = 1; k < 20; k++)
                    {
                        draw.shapes.AddCylinderClosed(new Point3D(i, j, k), new Vector3D(0.5, 0, 0), 0.2, 16);
                        draw.shapes.RecentShape.Color(Colors.Magenta);
                    }
                }
            }
            draw.RedrawShapes();
        }

        internal void DrawCicle()
        {

        }
        internal void DrawCicleCenterRadius()
        {
            
        }

        internal void AddLineFem(Point wP0, Point wP1)
        {
            Point3D p0 = draw.GetPoint3dOnBasePlane_FromPoint2D(wP0);
            Point3D p1 = draw.GetPoint3dOnBasePlane_FromPoint2D(wP1);
            Node n1 = fem.model.nodes.Add(p0);
            Node n2 = fem.model.nodes.Add(p1);
            fem.model.elems.AddFrame(n1, n2);
        }


        public void EraseAll()
        {
            fem.Initialize();
            RedrawFemModel();
        }
        internal void EraseSelected()
        {
            fem.DeleteSelection();
            RedrawFemModel();
        }
        internal void EraseFence()
        {
            requestUserInput = new RequestUserInput(this);
            requestUserInput.RequestPoints("define fence");
            requestUserInput.viewType = DRAW.SelectionWindow.ViewType.Line;
            requestUserInput.actionEveryLastTwoPointsWithPointPoint += SelectElemByFenceLine;
            requestUserInput.actionEnd += EraseSelected;
            requestUserInput.Start();
        }
        private void SelectElemByFenceLine(Point3D p0, Point3D p1)
        {
            Point3D pos = draw.PCamera.Position;
            Vector3D v0 = p0 - pos;
            Vector3D v1 = p1 - pos;

            //draw.GetInfiniteTriangleBySelectionFence(p0, p1, ref pos, ref v0, ref v1);
            fem.SelectByInfiniteTriangle(pos, v0, v1);

            RedrawFemModel();
        }

        private void FemDivide(object sender, RoutedEventArgs e)
        {
            cmd.Call("Divide");
        }
        internal void DivideElem()
        {
            requestUserInput = new RequestUserInput(this);
            requestUserInput.RequestElemSelection(R.String1);
            requestUserInput.RequestInt(R.String3);
            requestUserInput.actionAfterIntWithInt += fem.Divide;
            requestUserInput.actionEnd += fem.selection.Clear;
            requestUserInput.actionEnd += RedrawFemModel;
            requestUserInput.Start();
        }
        private void FemExtrude(object sender, RoutedEventArgs e)
        {
            cmd.Call("Extrude");
        }
        internal void ExtrudeElem()
        {
            requestUserInput = new RequestUserInput(this);
            requestUserInput.RequestElemSelection(R.String4);
            requestUserInput.RequestDirection(R.String5);
            requestUserInput.RequestInt(R.String6);
            requestUserInput.actionAfterIntWithDirInt += fem.ExtrudeWoRetern;
            requestUserInput.actionEnd += fem.selection.Clear;
            requestUserInput.actionEnd += RedrawFemModel;
            requestUserInput.Start();
        }
        internal void AddLineFem3D(Point3D p0, Point3D p1)
        {
            Node n1 = fem.model.nodes.Add(p0);
            Node n2 = fem.model.nodes.Add(p1);
            fem.model.elems.AddFrame(n1, n2);
        }
        internal void AddLine()
        {
            requestUserInput = new RequestUserInput(this);
            requestUserInput.viewType = DRAW.SelectionWindow.ViewType.Line;
            requestUserInput.RequestPoints(R.String7);
            requestUserInput.actionEveryLastTwoPointsWithPointPoint += AddLineFem3D;
            requestUserInput.actionEveryLastTwoPoints += RedrawFemModel;
            requestUserInput.Start();
        }
    }

    internal class UserInputAction
    {
        internal enum RequestInputType
        {
            Point,
            TwoPoints,
            Points,
            ElemSelection,
            NodeSelection,
            Selection,
            Int,
            Ints,
            Double,
            Distance,
            Direction,
        }
        internal RequestInputType requestInputType;
        internal string message;
        internal bool hasAction;
        internal int numPointRequested = -1;
        internal DRAW.SelectionWindow.ViewType viewType;
    }
    public class RequestUserMouseWindowInput

    {
        private readonly MainWindow main;
        private Point p0, p1;

        internal delegate void Action(Point p0, Point p1);
        internal Action action;
        private bool hasFirstPoint = false;
        private Point firstPoint;
        internal DRAW.SelectionWindow.ViewType viewType;

        internal Point FirstPoint
        {
            get
            {
                return firstPoint;
            }
            set
            {
                hasFirstPoint = true;
                firstPoint = value;
            }
        }

        public RequestUserMouseWindowInput(MainWindow main)
        {
            this.main = main;
        }
        internal void Start()
        {
            main.WindowSelectionOn(false);
            main.draw.selectionWindow.viewType = viewType;

            if (hasFirstPoint)
            {
                // 사용자 입력 윈도우의 첫번째 포인트가 이미 입력된 경우.
                if (main.orbiting) return;
                p0 = FirstPoint;
                main.draw.selectionWindow.Start(p0);
                main.MouseMove += WindowSelection_MouseMove;
                main.MouseUp += WindowSelection_MouseLeftUp;
                main.MouseLeave += WindowSelectionEnd;
            }
            else
            {
                main.MouseDown += WindowSelection_MouseLeftDown;
            }
        }
        private void WindowSelection_MouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            if (main.orbiting) return;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                p0 = e.GetPosition(main.grdMain);
                main.draw.selectionWindow.Start(p0);
                main.MouseMove += WindowSelection_MouseMove;
                main.MouseUp += WindowSelection_MouseLeftUp;
                main.MouseLeave += WindowSelectionEnd;
            }
        }
        private void WindowSelection_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            p1 = e.GetPosition(main.grdMain);
            main.draw.selectionWindow.Move(p1);
            //bckD.DrawSelectionWindow(selectWindowStart, selectWindowEnd);
        }
        private void WindowSelection_MouseLeftUp(object sender, MouseButtonEventArgs e)
        {
            WindowSelectionEnd(null, null);

            if (p0.Equals(p1)) return; //사각형 크기가 0인 경우 pass~

            action(p0, p1);
            //main.bckD.GetInfinitePyramidBySelectionWindow(selectWindowStart, selectWindowEnd, ref p0, ref v0, ref v1, ref v2, ref v3);
            //main.fem.SelectByInfinitePyramid(p0, v0, v1, v2, v3);
            main.RedrawFemModel();
        }
        private void WindowSelectionEnd(object sender, System.Windows.Input.MouseEventArgs e)
        {
            main.draw.selectionWindow.End();
            main.MouseMove -= WindowSelection_MouseMove;
            main.MouseUp -= WindowSelection_MouseLeftUp;
            main.MouseLeave -= WindowSelectionEnd;
            main.MouseDown -= WindowSelection_MouseLeftDown;

            main.WindowSelectionOn(true);
        }
    } //개체 선택 사용자 입력

    public partial class MainWindow : Window
    {
        private void OpenPannelCameraControl(object sender, RoutedEventArgs e)
        {
            dkpCameraControl.Visibility = Visibility.Visible;
            GetCameraInfo();
        }
        private void ClosePannelCameraControl(object sender, RoutedEventArgs e)
        {
            dkpCameraControl.Visibility = Visibility.Collapsed;
        }
        private void GetCameraInfo()
        {
            tbxCameraPositionX.Text = draw.PCamera.Position.X.ToString();
            tbxCameraPositionY.Text = draw.PCamera.Position.Y.ToString();
            tbxCameraPositionZ.Text = draw.PCamera.Position.Z.ToString();
            tbxCameraLookDirectionX.Text = draw.PCamera.LookDirection.X.ToString();
            tbxCameraLookDirectionY.Text = draw.PCamera.LookDirection.Y.ToString();
            tbxCameraLookDirectionZ.Text = draw.PCamera.LookDirection.Z.ToString();
            tbxCameraUpDirectionX.Text = draw.PCamera.UpDirection.X.ToString();
            tbxCameraUpDirectionY.Text = draw.PCamera.UpDirection.Y.ToString();
            tbxCameraUpDirectionZ.Text = draw.PCamera.UpDirection.Z.ToString();
            tbxCameraFarPlaneDistance.Text = draw.PCamera.FarPlaneDistance.ToString();
            tbxCameraFieldOfView.Text = draw.PCamera.FieldOfView.ToString();
            tbxCameraNearPlaneDistance.Text = draw.PCamera.NearPlaneDistance.ToString();
        }
        private void SetCameraInfo(object sender, RoutedEventArgs e)
        {
            draw.PCamera.Position = new Point3D
            {
                X = Convert.ToDouble(tbxCameraPositionX.Text),
                Y = Convert.ToDouble(tbxCameraPositionY.Text),
                Z = Convert.ToDouble(tbxCameraPositionZ.Text)
            };
            draw.PCamera.LookDirection = new Vector3D
            {
                X = Convert.ToDouble(tbxCameraLookDirectionX.Text),
                Y = Convert.ToDouble(tbxCameraLookDirectionY.Text),
                Z = Convert.ToDouble(tbxCameraLookDirectionZ.Text)
            };
            draw.PCamera.UpDirection = new Vector3D
            {
                X = Convert.ToDouble(tbxCameraUpDirectionX.Text),
                Y = Convert.ToDouble(tbxCameraUpDirectionY.Text),
                Z = Convert.ToDouble(tbxCameraUpDirectionZ.Text)
            };
            draw.PCamera.FieldOfView = Convert.ToDouble(tbxCameraFieldOfView.Text);
            draw.PCamera.NearPlaneDistance = Convert.ToDouble(tbxCameraNearPlaneDistance.Text);
            draw.PCamera.FarPlaneDistance = Convert.ToDouble(tbxCameraFarPlaneDistance.Text);
        }

        private void OpenPannelConcreteSetting(object sender, RoutedEventArgs e)
        {
            dkpStructureConcrete.Visibility = Visibility.Visible;
            tbxHeight.Text = smr.structure.height.ToString();
            tbxLength.Text = smr.structure.length.ToString();
            tbxWidth.Text = smr.structure.width.ToString();
        }
        private void ClosePannelConcreteSetting(object sender, RoutedEventArgs e)
        {
            dkpStructureConcrete.Visibility = Visibility.Collapsed;
        }
        private void SetConcrete(object sender, RoutedEventArgs e)
        {
            smr.structure.height = Convert.ToDouble(tbxHeight.Text);
            smr.structure.length = Convert.ToDouble(tbxLength.Text);
            smr.structure.width = Convert.ToDouble(tbxWidth.Text);
            draw.shapes.AddBox(new Point3D(0, 0, 0), new Vector3D(smr.structure.length, smr.structure.width, smr.structure.height));
            draw.RedrawShapes();
        }

        private void OpenPannelFemWorks(object sender, RoutedEventArgs e)
        {
            dkpFemWorks.Visibility = Visibility.Visible;
            RedrawFemWorksTreeView();
        }
        private void ClosePannelFemWorks(object sender, RoutedEventArgs e)
        {
            dkpFemWorks.Visibility = Visibility.Collapsed;
        }
        private void RedrawFemWorksTreeView()
        {
            //ref. https://www.codeproject.com/Articles/124644/Basic-Understanding-of-Tree-View-in-WPF
            treeViewFemWorks.Items.Clear();

            TreeViewItem item = new TreeViewItem();
            item.Header = "Materials(" + fem.model.materials.Count + ")";
            item.IsExpanded = false;
            foreach (MaterialFem material in fem.model.materials)
            {
                item.Items.Add(new TreeViewItem() { Header = material.name });
            }
            treeViewFemWorks.Items.Add(item);

            item = new TreeViewItem();
            item.Header = "Sections(" + fem.model.sections.Count + ")";
            item.IsExpanded = false;
            foreach (Section section in fem.model.sections)
            {
                item.Items.Add(new TreeViewItem() { Header = section.num });
            }
            treeViewFemWorks.Items.Add(item);

            item = new TreeViewItem();
            item.Header = "Nodes(" + fem.model.nodes.Count + ")";
            item.IsExpanded = true;
            treeViewFemWorks.Items.Add(item);

            item = new TreeViewItem();
            Elements elems = fem.model.elems;
            item.Header = "Elements(" + elems.Count + ")";
            item.IsExpanded = true;
            if (elems.frames.Count != 0) item.Items.Add(new TreeViewItem() { Header = "Frame(" + elems.frames.Count + ")" });
            if (elems.plates.Count != 0) item.Items.Add(new TreeViewItem() { Header = "Plate(" + elems.plates.Count + ")" });
            if (elems.solids.Count != 0) item.Items.Add(new TreeViewItem() { Header = "Solid(" + elems.solids.Count + ")" });
            treeViewFemWorks.Items.Add(item);

            item = new TreeViewItem();
            item.Header = "Boundaries(" + fem.model.boundaries.Count + ")";
            item.IsExpanded = false;
            foreach (Boundary boundary in fem.model.boundaries)
            {
                item.Items.Add(new TreeViewItem() { Header = boundary.node.num });
            }
            treeViewFemWorks.Items.Add(item);

            item = new TreeViewItem();
            item.Header = "Loads(" + fem.loads.Count + ")";
            item.IsExpanded = false;
            foreach (Boundary boundary in fem.model.boundaries)
            {
                item.Items.Add(new TreeViewItem() { Header = boundary.node.num });
            }
            treeViewFemWorks.Items.Add(item);


        }

        private void ViewCoordinateSystem(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem sd = (System.Windows.Controls.MenuItem)sender;
            draw.showCoordinateSystem = sd.IsChecked;
            draw.RedrawShapes();
        }
        private void ViewBasePlaneGrid(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem sd = (System.Windows.Controls.MenuItem)sender;
            draw.showBasePlaneGrid = sd.IsChecked;
            draw.RedrawShapes();
        }

        public void RedrawFemModel()
        {
            RedrawFemWorksTreeView();

            bool showUndeformedShape = true;
            bool showSection = true;

            double maxLoadLength = 2;
            double maxLoadSize = fem.loads.GetMaxLoadLength();
            double loadViewScale = maxLoadLength / maxLoadSize;

            draw.shapes.Clear();
            draw.texts.Clear();

            double diaNode;
            double diaElem;
            int rlsNode;
            int rlsElem;

            diaNode = 0.15;
            diaElem = 0.05;
            rlsNode = 12;
            rlsElem = 6;
            Color colorNode = Colors.Yellow;
            Color colorSelectedNode = Colors.Red;
            Color colorElem = Colors.Blue;
            Color colorSelectedElem = Colors.Red;
            Color colorLoad = Colors.Red;
            Color colorReaction = Colors.Yellow;

            //Deformation
            if (fem.solved)
            {
                if (fem.model.nodes.show)
                {
                    foreach (Node node in fem.model.nodes)
                    {
                        draw.shapes.AddSphere(node.c1, diaNode, rlsNode);
                        draw.shapes.RecentShape.Color(colorNode);
                    }
                }
                if (fem.model.nodes.showNumber)
                {
                    foreach (Node node in fem.model.nodes)
                    {
                        draw.texts.Add(node.num.ToString(), node.c1, 8);
                    }
                }
                if (fem.model.elems.show)
                {
                    foreach (Element e in fem.model.elems)
                    {
                        switch (e.type)
                        {
                            case 21:
                                Frame frame = (Frame)e;
                                Point3D str = frame.nodes[0].c1;
                                Point3D end = frame.nodes[1].c1;
                                Vector3D dir = end - str;
                                if (showSection)
                                {
                                    draw.shapes.AddPolygon(str, dir, frame.section.poly);
                                }
                                else
                                {
                                    draw.shapes.AddCylinder(str, dir, diaElem, rlsElem);
                                }
                                break;
                            case 40:
                                Plate p = (Plate)e;
                                draw.shapes.AddBox(p.nodes[0].c1, p.nodes[2].c1 - p.nodes[0].c1);

                                break;
                            case 80:
                                Solid s = (Solid)e;
                                draw.shapes.AddHexahedron(s.nodes[0].c1, s.nodes[1].c1, s.nodes[2].c1, s.nodes[3].c1, s.nodes[4].c1, s.nodes[5].c1, s.nodes[6].c1, s.nodes[7].c1);
                                break;
                            default:
                                draw.shapes.RecentShape.Color(colorElem);
                                break;
                        }
                    }
                }
                if (fem.model.elems.showNumber)
                {
                    foreach (Element elem in fem.model.elems)
                    {
                        draw.texts.Add(elem.num.ToString(), elem.Center, 8);
                    }
                }

                foreach (FemLoad load in fem.loads)
                {
                    foreach (NodalLoad nodalLoad in load.nodalLoads)
                    {
                        draw.shapes.AddForce(nodalLoad.node.c1, nodalLoad.force * loadViewScale);
                        draw.shapes.RecentShape.Color(colorLoad);
                    }
                }

                //Reaction Force
                foreach (Node node in fem.model.nodes)
                {
                    Vector3D dir = new Vector3D(node.reactionForce[0], node.reactionForce[1], node.reactionForce[2]);
                    draw.shapes.AddForce(node.c1, dir * loadViewScale);
                    draw.shapes.RecentShape.Color(colorReaction);
                }
            }

            //Undeformed
            if (showUndeformedShape)
            {
                bool showUndeformedForce;
                double opacity;

                if (fem.solved)
                {
                    colorNode = Colors.Gray;
                    colorElem = Colors.Gray;
                    colorLoad = Colors.Gray;
                    opacity = 0.2;
                    showUndeformedForce = false;
                }
                else
                {
                    opacity = 1;
                    showUndeformedForce = true;
                }

                if (!fem.solved)
                {
                    if (fem.model.nodes.show)
                    {
                        foreach (Node node in fem.model.nodes)
                        {
                            draw.shapes.AddSphere(node.c0, diaNode, rlsNode);
                            if (node.selected)
                            {
                                draw.shapes.RecentShape.Color(colorSelectedNode);
                            }
                            else
                            {
                                draw.shapes.RecentShape.Color(colorNode);
                            }
                            draw.shapes.RecentShape.Opacity(opacity);
                        }
                    }
                    if (fem.model.nodes.showNumber)
                    {
                        foreach (Node node in fem.model.nodes)
                        {
                            draw.texts.Add(node.num.ToString(), node.c0, 8);
                        }
                    }
                }
                if (fem.model.elems.show)
                {
                    foreach (Element e in fem.model.elems)
                    {
                        switch (e.type)
                        {
                            case 21:
                                Frame frame = (Frame)e;
                                Point3D str = frame.nodes[0].c0;
                                Point3D end = frame.nodes[1].c0;
                                Vector3D dir = end - str;

                                if(frame.section == null)
                                {
                                    draw.shapes.AddCylinder(str, dir, diaElem, rlsElem);
                                }
                                else
                                {
                                    if(showSection & frame.section.hasSectionPoly)
                                    {
                                        draw.shapes.AddPolygon(str, dir, frame.section.poly);
                                    }
                                    else
                                    {
                                        draw.shapes.AddCylinder(str, dir, diaElem, rlsElem);
                                    }
                                }
                                break;
                            case 40:
                                Plate p = (Plate)e;
                                //draw.shapes.AddBox(p.nodes[0].c0, p.nodes[2].c0 - p.nodes[0].c0);
                                draw.shapes.AddRectangle(p.nodes[0].c0, p.nodes[1].c0, p.nodes[2].c0, p.nodes[3].c0);
                                break;
                            case 80:
                                Solid s = (Solid)e;
                                draw.shapes.AddHexahedron(s.nodes[0].c0, s.nodes[1].c0, s.nodes[2].c0, s.nodes[3].c0, s.nodes[4].c0, s.nodes[5].c0, s.nodes[6].c0, s.nodes[7].c0);
                                break;
                            default:
                                break;
                        }
                        if (e.selected)
                        {
                            draw.shapes.RecentShape.Color(colorSelectedElem);
                        }
                        else
                        {
                            draw.shapes.RecentShape.Color(colorElem);
                        }
                        draw.shapes.RecentShape.Opacity(opacity);
                    }
                }
                if (showUndeformedForce)
                {
                    foreach (FemLoad load in fem.loads)
                    {
                        foreach (NodalLoad nodalLoad in load.nodalLoads)
                        {
                            draw.shapes.AddForce(load.nodalLoads[0].node.c0, nodalLoad.force * loadViewScale);
                            draw.shapes.RecentShape.Color(colorLoad);
                            draw.shapes.RecentShape.Opacity(opacity);
                        }
                    }
                }
            }

            draw.RedrawShapes();
        }
        private void RedrawFemModel(object sender, RoutedEventArgs e)
        {
            RedrawFemModel();
        }
        public void RedrawShapes(object sender, RoutedEventArgs e)
        {
            draw.RedrawShapes();
        }
        public void RedrawShapes()
        {
            RedrawShapes(null,null);
        }

    } // 패널, 격자배경, 좌표계, Redraw 등
    public partial class MainWindow : Window
    {
        private Point pointMouseDown;
        internal bool orbiting = false;

        private bool windowSelectionOn = false;
        internal void WindowSelectionOn(bool on)
        {
            if (orbiting) return;
            if (on)
            {
                if (!windowSelectionOn) //이벤트 중복생성 방지
                {
                    MouseDown += WindowSelection_MouseLeftDown;
                }
            }
            else
            {
                MouseDown -= WindowSelection_MouseLeftDown;
            }
            windowSelectionOn = on;
        }
        internal void TurnOnDeselectAll_Esc(bool on)
        {
            if (on)
            {
                KeyDown += UnselectAll_Esc;
            }
            else
            {
                KeyDown -= UnselectAll_Esc;
            }
        }
        private void TurnOnErase_Del(bool on)
        {
            if (on)
            {
                PreviewKeyDown += Erase_Del;
            }
            else
            {
                PreviewKeyDown -= Erase_Del;
            }
        }
        private void WindowSelection_MouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            if (orbiting) return;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                RequestUserMouseWindowInput r = new RequestUserMouseWindowInput(this);
                r.viewType = DRAW.SelectionWindow.ViewType.SelectionWindow;
                r.FirstPoint = e.GetPosition(grdMain);
                r.action = SelectFemByWindow;
                r.Start();

                //requestUserSelectionWindow.Reset(2);
                //requestUserSelectionWindow.viewType = DRAW.SelectionWindow.ViewType.SelectionWindow;
                //requestUserSelectionWindow.Put(e.GetPosition(grdMain));
                //requestUserSelectionWindow.actionEveryLastTwoPoints = SelectFemByWindow;
                //requestUserSelectionWindow.Start();
            }
        }
        private void UnselectAll_Esc(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                fem.selection.DeselectAll();
                RedrawFemModel();
            }
        }
        private void Erase_Del(object sender, System.Windows.Input.KeyEventArgs e)
        {
             if (e.Key == Key.Delete)
            {
                fem.selection.Delete();
                RedrawFemModel();
            }
        }
        public void SelectFemByWindow(Point wP0, Point wP1)
        {
            if (wP0.Equals(wP1)) return; //사각형 크기가 0인 경우 pass~

            Point3D p0 = new Point3D();
            Vector3D v0 = new Vector3D();
            Vector3D v1 = new Vector3D();
            Vector3D v2 = new Vector3D();
            Vector3D v3 = new Vector3D();

            //pespective view에서 사각형을 그리는 방법으로 개체를 선택하려면 카메라의 위치 p0에서 각 모서리의 벡터 v0~v3을 얻어야 함.
            //draw에 사용자가 그린 사각형 정보 wP0, wP1을 전달해서 피라미드 정보를 얻어와서. fem에서 요소와 절점을 선택하도록 지시함.
            draw.GetInfinitePyramidBySelectionWindow(wP0, wP1, ref p0, ref v0, ref v1, ref v2, ref v3);
            if (wP0.X > wP1.X)
            {
                //사각형을 반대방향으로 그린 경우 경계선에 겹친 모든 요소를 선택함.
                fem.SelectByInfinitePyramid_Cross(p0, v0, v1, v2, v3);
            }
            else
            {
                fem.SelectByInfinitePyramid(p0, v0, v1, v2, v3);
            }
        }
    } // 마우스 이벤트 관련
    public partial class MainWindow : Window
    {
        private void FullScreenWindow(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem sd = (System.Windows.Controls.MenuItem)sender;
            switch (sd.IsChecked)
            {
                case true:
                    WindowState = System.Windows.WindowState.Maximized;
                    break;
                case false:
                    WindowState = System.Windows.WindowState.Normal;
                    break;
            }
            draw.RedrawShapes();
        }

        private void ViewSW(object sender, RoutedEventArgs e)
        {
            draw.ViewSW();
        }
        private void ViewSE(object sender, RoutedEventArgs e)
        {
            draw.ViewSE();
        }
        private void ViewNW(object sender, RoutedEventArgs e)
        {
            draw.ViewNW();
        }
        private void ViewNE(object sender, RoutedEventArgs e)
        {
            draw.ViewNE();
        }
        private void ViewTop(object sender, RoutedEventArgs e)
        {
            draw.ViewTop();
        }
        private void ViewFront(object sender, RoutedEventArgs e)
        {
            draw.ViewFront();
        }
        private void ViewBottom(object sender, RoutedEventArgs e)
        {
            draw.ViewBottom();
        }
        private void ViewLeft(object sender, RoutedEventArgs e)
        {
            draw.ViewLeft();
        }
        private void ViewRight(object sender, RoutedEventArgs e)
        {
            draw.ViewRight();
        }
        private void ViewBack(object sender, RoutedEventArgs e)
        {
            draw.ViewBack();
        }

        public void ZoomExtents(object sender, RoutedEventArgs e)
        {
            draw.ViewZoomExtend();
        }
        public void ZoomExtents()
        {
            ZoomExtents(null, null);
        }
        internal void ZoomAll()
        {
            ZoomExtents(null, null);
        }
        internal void ZoomWindow()
        {
            requestUserInput = new RequestUserInput(this);
            requestUserInput.viewType = DRAW.SelectionWindow.ViewType.Rectangle;
            requestUserInput.RequestPoints(2);
            requestUserInput.actionEveryLastTwoPointsWithPointPoint += draw.ViewZoomWindow;
            requestUserInput.Start();
        }

        private void TurnOnWheelPanZoom()
        {
            MouseWheel += new MouseWheelEventHandler(Zoom_MouseWheelScroll);
            MouseDown += PanOn_MouseWheelDown;
            MouseUp += PanOff_MouseWheelUp;
        }
        private void Zoom_MouseWheelScroll(object sender, MouseWheelEventArgs e)
        {
            draw.ZoomForward(e.Delta);
            GetCameraInfo();
        }
        private void PanOn_MouseWheelDown(object sender, MouseButtonEventArgs e)
        {
            if(e.MiddleButton == MouseButtonState.Pressed)
            {
                pointMouseDown = e.GetPosition(grdMain);
                draw.OrbitStart();
                if (requestUserInput.On)
                {
                    //requestUserInput.PanMoveStart();
                }
                MouseMove += Pan_MouseWheelDownMove;
            }
        }
        private void Pan_MouseWheelDownMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                Point p = e.GetPosition(grdMain);
                Vector mov = p - pointMouseDown;
                stbLabel.Content = mov.X + ", " + mov.Y;
                //bckD.OrbitMoveX(mov.X / 2); //MoveX와 MoveY중 처음 실행된 것 하나만 동작함.
                //bckD.OrbitMoveY(mov.Y / 2);
                draw.OrbitMove(mov);
                if (requestUserInput.On)
                {
                    //requestUserInput.PanMove(mov);
                }
                GetCameraInfo();
            }
        }
        private void PanOff_MouseWheelUp(object sender, MouseButtonEventArgs e)
        {
            draw.OrbitEnd();
            if (requestUserInput.On)
            {
                //requestUserInput.PanMoveEnd();
            }
            MouseMove -= Pan_MouseWheelDownMove;
        }

        private void TurnOnOrbit(object sender, RoutedEventArgs e)
        {
            TurnOnOrbit(true);
        }
        internal void TurnOnOrbit(bool on)
        {
            if (on)
            {
                orbiting = true;
                MouseDown += new MouseButtonEventHandler(OrbitStart_MouseDown);
                MouseUp += new MouseButtonEventHandler(OrbitEnd_MouseUp);
                grdMain.MouseLeave += new System.Windows.Input.MouseEventHandler(Orbit_MouseLeave);
                KeyDown += TurnOffOrbit_Esc;
                TurnOnDeselectAll_Esc(false);
            }
            else
            {
                OritEnd();
            }

        }
        private void OrbitStart_MouseDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            pointMouseDown = e.GetPosition(grdMain);
            draw.OrbitStart();
            stbLabel.Content = "0, 0";
            MouseMove += new System.Windows.Input.MouseEventHandler(Orbit_MouseMove);
        }
        private void Orbit_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point p = e.GetPosition(grdMain);
                Vector mov = p - pointMouseDown;
                stbLabel.Content = mov.X + ", " + mov.Y;

                draw.OrbitRotate(mov);
                GetCameraInfo();
            }
            else
            {
                //Point p = e.GetPosition(grdMain);
                //stbLabel.Content = (p.X) + ", " + (p.Y);
            }
            if (e.RightButton == MouseButtonState.Pressed)
            {
                //Point strPoint_grdMain = grdMain.TransformToAncestor(baseDockPanel).Transform(new Point(0, 0));
                Point strPoint_grdMain = new Point(0, 0);
                Point center_gridMain = new Point(strPoint_grdMain.X + grdMain.ActualWidth / 2, strPoint_grdMain.Y + grdMain.ActualHeight / 2);
                Point p = e.GetPosition(grdMain);

                //bckD.DrawLine(strPoint_grdMain, center_gridMain);

                Vector center2mouseDownPoint;
                center2mouseDownPoint = pointMouseDown - center_gridMain;
                //bckD.DrawLine(center_gridMain, pointMouseDown);
                double iniDist = center2mouseDownPoint.Length;

                Vector center2point = p - center_gridMain;
                double newDist = center2point.Length;

                double dist = newDist - iniDist;

                double iniAngle = -Math.Atan2(center2mouseDownPoint.Y, center2mouseDownPoint.X);
                double newAngle = -Math.Atan2(center2point.Y, center2point.X);

                double rad = newAngle - iniAngle;

                stbLabel.Content = "d = " + dist + ", rad = " + rad;
                draw.OrbitTwist(rad, dist);
                GetCameraInfo();
            }
        }
        private void Orbit_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            stbLabel.Content = "";
        }
        private void OrbitEnd_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //stbLabel.Content = (p.X) + ", " + (p.Y);
            stbLabel.Content = "";
            draw.OrbitEnd();
            this.MouseMove -= new System.Windows.Input.MouseEventHandler(Orbit_MouseMove);
        }
        private void TurnOffOrbit_Esc(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                OritEnd();
            }
        }
        private void OritEnd()
        {
            orbiting = false;
            MouseDown -= new MouseButtonEventHandler(OrbitStart_MouseDown);
            MouseUp -= new MouseButtonEventHandler(OrbitEnd_MouseUp);
            grdMain.MouseLeave -= new System.Windows.Input.MouseEventHandler(Orbit_MouseLeave);
            KeyDown -= TurnOffOrbit_Esc;
            TurnOnDeselectAll_Esc(true);
        }

        private void ViewNode(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem sd = (System.Windows.Controls.MenuItem)sender;
            fem.model.nodes.show = sd.IsChecked;
            RedrawFemModel();
        }
        private void ViewNodeNumber(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem sd = (System.Windows.Controls.MenuItem)sender;
            fem.model.nodes.showNumber = sd.IsChecked;
            RedrawFemModel();
        }
        private void ViewElement(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem sd = (System.Windows.Controls.MenuItem)sender;
            fem.model.elems.show = sd.IsChecked;
            RedrawFemModel();
        }
        private void ViewElementNumber(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem sd = (System.Windows.Controls.MenuItem)sender;
            fem.model.elems.showNumber = sd.IsChecked;
            RedrawFemModel();
        }
    } // View 관련
}