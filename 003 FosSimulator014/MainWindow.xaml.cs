using BCK.SmrSimulation.Draw2D;
using BCK.SmrSimulation.Draw3D;
using BCK.SmrSimulation.finiteElementMethod;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Forms.PropertyGridInternal;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using bck.SMR_simulator.main.Properties;

namespace BCK.SmrSimulation.Main
{
    partial class MainWindow : Window
    {
        private readonly SMR smr;
        public FEM Fem => fem;
        private readonly FEM fem;

        public BckDraw3D Draw => draw;
        private readonly BckDraw3D draw;
        
        internal readonly Draw2D.Draw2D draw2d;
        internal MouseInputGuideShapes mouseInputGuideShapes;

        public CommandWindow Cmd => cmd;
        private readonly CommandWindow cmd;
        internal RequestUserInput requestUserInput;

        public CultureInfo CultureInfo => cultureInfo;


        private readonly CultureInfo cultureInfo = new CultureInfo("ko-KR", false);

        public MainWindow()
        {
            InitializeComponent();

            cmd = new CommandWindow(this, tbxCommand);
            smr = new SMR();
            fem = new FEM();
            fem.cmd = cmd; //fem에서 메시지를 보내기위한 용도로만 사용함.

            draw = new Draw3D.BckDraw3D(grdMain);
            draw2d = new Draw2D.Draw2D(grdMain);

            mouseInputGuideShapes = new MouseInputGuideShapes(grdMain);
            requestUserInput = new RequestUserInput(this);

            InitializeSetting();
            EventSetter();
            FunctionKeyClickEvent();

            draw.ViewTop();
            draw.ViewZoomExtend();

            //TestNodeGrid();
            //TestExtrude();
            //TwiceExtrudeTest();
            //Drawing2dTest();
            BoundaryConditionDrawingTest();

            //RedrawFemModel();

        }

        private void InitializeSetting()
        {

            //Fem Node번호 보일지 말지 결정하는 옵션
            isFemViewNodeNumber.IsChecked = Settings.Default.isFemViewNodeNumber;
            isFemViewElemNumber.IsChecked = Settings.Default.isFemViewElemNumber;

            SetObjectSnap(Settings.Default.isOnObjectSnap);
            SetOrthogonalOption(Settings.Default.isOnOrthogonal);

        }
        private void EventSetter()
        {
            grdMain.SizeChanged += GrdMain_SizeChanged; // gird size가 변경된 경우 redraw3dRelated2dShapes 수행.
            TurnOnWheelPanZoom();
            IsOnWindowSelect = true;
            IsOnDeselectAll_Esc = true;
            TurnOnErase_Del(true);
            TurnOnFemAnalysis_F5(true);
        }
        private void FunctionKeyClickEvent()
        {
            this.KeyDown += MainWindow_KeyDown_FunctionKeys;
        }
        private void MainWindow_KeyDown_FunctionKeys(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F1:
                    break;
                case Key.F2:
                    break;
                case Key.F3:
                    break;
                case Key.F4:
                    break;
                case Key.F5:
                    break;
                case Key.F6:
                    SwitchObjectSnap(null, null);
                    break;
                case Key.F7:
                    break;
                case Key.F8:
                    SwitchOrthogonal(null, null);
                    break;
                case Key.F9:
                    break;
                case Key.F10:
                    break;
                case Key.System:
                    if(e.SystemKey == Key.F10)
                    {
                        //F10은 특이하다고 함.
                        //ref. https://stackoverflow.com/questions/2103497/detecting-the-user-pressing-f10-in-wpf

                    }
                    break;
                case Key.F11:
                    break;
                case Key.F12:
                    break;
                default:
                    break;
            }
        }


        private void GrdMain_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RedrawShapesWo3dGeneration();
        }

        private void BoundaryConditionDrawingTest()
        {
            Cmd.Call("Erase");
            Cmd.Call("All");
            Cmd.Call("Line");
            Cmd.Call("0,0");
            Cmd.Call("10,0");
            Cmd.Call(" ");
            Cmd.Call("Select");
            Cmd.Call("Element");
            Cmd.Call("1");
            Cmd.Call("Divide");
            Cmd.Call("10");

            Cmd.Call("select");
            Cmd.Call("node");
            Cmd.Call("1");

            Cmd.Call("select");
            Cmd.Call("node");
            Cmd.Call("2");

            Cmd.Call("boundary");
            Cmd.Call("f");

            Cmd.Call("re");

        }
        public void Drawing2dTest()
        {
            Point p0 = new Point(100, 100);
            Point p1 = new Point(200, 300);
            draw2d.shapes.lines.Add(p0, p1);
            //p1 = new Point(400, 400);
            int[] boundaryCondition = new int[6];
            boundaryCondition[0] = 1;
            boundaryCondition[1] = 1;
            boundaryCondition[2] = 0;
            boundaryCondition[3] = 1;
            boundaryCondition[4] = 1;
            boundaryCondition[5] = 1;

            draw2d.boundaryConditionMarks.Add(p0, boundaryCondition);

        }
        public bool TwiceExtrudeTest()
        {
            Cmd.Call("Erase");
            Cmd.Call("All");
            Cmd.Call("Line");
            Cmd.Call("0,0");
            Cmd.Call("10,0");
            Cmd.Call(" ");
            Cmd.Call("Select");
            Cmd.Call("Element");
            Cmd.Call("1");
            Cmd.Call("Extrude");
            Cmd.Call("@0,1");
            Cmd.Call("5");

            if (Fem.Model.Elems.Count != 5) return false;

            Cmd.Call("Erase");
            Cmd.Call("All");
            Cmd.Call("Line");
            Cmd.Call("0,0");
            Cmd.Call("10,0");
            Cmd.Call(" ");
            Cmd.Call("Select");
            Cmd.Call("Element");
            Cmd.Call("1");
            Cmd.Call("Extrude");
            Cmd.Call("@0,1");
            Cmd.Call("5");

            if (Fem.Model.Elems.Count != 5) return false;

            return true;
        }

        private void TestExtrude()
        {
            Fem.Initialize();
            FemMaterial matl1 = Fem.Model.Materials.AddConcrete("C30");
            FemSection sect1 = Fem.Model.Sections.AddRectangle(1, 0.2);

            FemNode n1 = Fem.Model.Nodes.Add(0, 0, 0);
            FemNode n2 = Fem.Model.Nodes.Add(10, 0, 0);
            FemFrame f = Fem.Model.Elems.AddFrame(n1, n2);
            f.Material = matl1;
            f.Section = sect1;
            Fem.Select(f);
            Fem.DivideSelectedElems(2);

            Fem.SelectElemAll();
            Fem.DivideSelectedElems(2);

            Fem.SelectElemAll();
            Fem.DivideSelectedElems(2);

            Fem.SelectElemAll();
            Vector3D dir = new Vector3D(0, 1, 0);
            Fem.ExtrudeSelectedElems(dir, 5);

            Fem.SelectElemAll();
            dir = new Vector3D(0, 0, 1);
            Fem.ExtrudeSelectedElems(dir, 5);

        }
        private void TestNodeGrid()
        {
            for(int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    Point3D p0 = new Point3D(i, j, 0);

                    Fem.Model.Nodes.Add(i, j, 0);
                }
            }
        }

        private void FemTest_Solid003(object sender, RoutedEventArgs e)
        {
            Fem.Initialize();
            FemMaterial matl1 = Fem.Model.Materials.AddConcrete("C30");
            FemSection sect1 = Fem.Model.Sections.AddRectangle(1, 0.2);

            FemNode n1 = Fem.Model.Nodes.Add(0, 0, 0);
            FemNode n2 = Fem.Model.Nodes.Add(2, 0, 0);
            FemFrame f = Fem.Model.Elems.AddFrame(n1, n2);
            f.Material = matl1;
            f.Section = sect1;
            Fem.Select(f);
            Fem.DivideSelectedElems(2);

            Fem.SelectElemAll();
            Vector3D dir = new Vector3D(0, 1, 0);
            FemElementCollection ee = Fem.ExtrudeSelectedElems(dir, 2);
            
            dir = new Vector3D(0, 0, 0.5);
            Fem.Extrude(ee, dir, 20);
            
            Fem.Model.Boundaries.AddBoundary(Fem.Model.Nodes[0], 1, 1, 1, 1, 1, 1);
            Fem.Model.Boundaries.AddBoundary(Fem.Model.Nodes[1], 1, 1, 1, 1, 1, 1);
            Fem.Model.Boundaries.AddBoundary(Fem.Model.Nodes[2], 1, 1, 1, 1, 1, 1);
            Fem.Model.Boundaries.AddBoundary(Fem.Model.Nodes[3], 1, 1, 1, 1, 1, 1);
            Fem.Model.Boundaries.AddBoundary(Fem.Model.Nodes[4], 1, 1, 1, 1, 1, 1);
            Fem.Model.Boundaries.AddBoundary(Fem.Model.Nodes[5], 1, 1, 1, 1, 1, 1);
            Fem.Model.Boundaries.AddBoundary(Fem.Model.Nodes[6], 1, 1, 1, 1, 1, 1);
            Fem.Model.Boundaries.AddBoundary(Fem.Model.Nodes[7], 1, 1, 1, 1, 1, 1);
            Fem.Model.Boundaries.AddBoundary(Fem.Model.Nodes[8], 1, 1, 1, 1, 1, 1);
            
            Vector3D force = new Vector3D(1000, 0, 0);
            Vector3D moment = new Vector3D(0, 0, 0);
            
            FemNode np = Fem.Model.Nodes.GetNode(188);
            if (np != null)
            {
                Fem.Loads.AddNodal(np, force, moment);
            }


            //double[,] mat = new double[3, 3];
            //mat[0, 0] = 1; mat[0, 1] = 2; mat[0, 2] = 2;
            //mat[1, 0] = 1; mat[1, 1] = 2; mat[1, 2] = 3;
            //mat[2, 0] = 2; mat[2, 1] = 2; mat[2, 2] = 3;
            ////double[,] mat = new double[2, 2];
            ////mat[0, 0] = 1; mat[0, 1] = 2;
            ////mat[1, 0] = 1; mat[1, 1] = 1;
            //double det = GF.Determinant(mat);


            Fem.Solve();
            RedrawFemModel();
        }
        private void FemTest_SimpleBeamLoadZ(object sender, RoutedEventArgs e)
        {
            Fem.Initialize();
            FemMaterial matl1 = Fem.Model.Materials.AddConcrete("C30");

            double width = 1;
            double height = 0.2;
            FemSection sect1 = Fem.Model.Sections.AddRectangle(width, height);

            FemNode n1 = Fem.Model.Nodes.Add(0, 0, 0);
            FemNode n2 = Fem.Model.Nodes.Add(10, 0, 0);
            FemFrame f = Fem.Model.Elems.AddFrame(n1, n2);
            f.Material = matl1;
            f.Section = sect1;
            
            Fem.Select(f);
            Fem.DivideSelectedElems(10);

            Fem.Model.Boundaries.AddBoundary(n1, 1, 1, 1, 1, 0, 0);
            Fem.Model.Boundaries.AddBoundary(n2, 0, 1, 1, 0, 0, 0);

            Vector3D force = new Vector3D(0, 0, -50);
            Vector3D moment = new Vector3D(0, 0, 0);

            FemNode np = Fem.Model.Nodes.GetNode(4);
            Fem.Loads.AddNodal(np, force, moment);

            Fem.Solve();

            RedrawFemModel();
        }
        private void FemTest_SimpleBeamLoadY(object sender, RoutedEventArgs e)
        {
            Fem.Initialize();
            FemMaterial matl1 = Fem.Model.Materials.AddConcrete("C30");

            double width = 1;
            double height = 0.2;
            FemSection sect1 = Fem.Model.Sections.AddRectangle(width, height);

            FemNode n1 = Fem.Model.Nodes.Add(0, 0, 0);
            FemNode n2 = Fem.Model.Nodes.Add(10, 0, 0);
            FemFrame f = Fem.Model.Elems.AddFrame(n1, n2);
            f.Material = matl1;
            f.Section = sect1;

            Fem.Select(f);
            Fem.DivideSelectedElems(10);

            Fem.Model.Boundaries.AddBoundary(n1, 1, 1, 1, 1, 0, 0);
            Fem.Model.Boundaries.AddBoundary(n2, 0, 1, 1, 0, 0, 0);

            Vector3D force = new Vector3D(0, 600, 0);
            Vector3D moment = new Vector3D(0, 0, 0);

            FemNode np = Fem.Model.Nodes.GetNode(7);
            Fem.Loads.AddNodal(np, force, moment);

            Fem.Solve();

            RedrawFemModel();
        }

        private void ExitApplication(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();
            this.Close();
        }

        private void DrawSampleGradient(object sender, RoutedEventArgs e)
        {
            Draw.ExampleDrawGradient3D();
        }
        private void DrawCone(object sender, RoutedEventArgs e)
        {
            double radius = 5;
            Point3D center = new Point3D(-1, -1, -1);
            Vector3D heightVector = new Vector3D(10,0, 0);
            //bckD.DrawCone(center, radius, heightVector, resolution, Colors.AliceBlue);
            Draw.Shapes.AddCone(radius, heightVector, center, 6);
            Draw.RegenerateShapesModelVisual3ds();
            Draw.RedrawShapes();
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
            Draw.Shapes.AddCylinderClosed(str, dir, dia, resolution);
            Draw.Shapes.RecentShape.Color(Colors.Red);


            dir = new Vector3D(0, len, 0);
            Draw.Shapes.AddCylinderClosed(str, dir, dia, resolution);
            Draw.Shapes.RecentShape.Color(Colors.Green);

            dir = new Vector3D(0, 0, len);
            //bckD.DrawCylinderClosed(str, dir, dia, resolution, Colors.Black);
            Draw.Shapes.AddCylinderClosed(str, dir, dia, resolution);
            Draw.Shapes.RecentShape.Color(Colors.Black);
            //bckD.shapes.AddBox(new Point3D(0, 0, 0), new Vector3D(10, 10, 10));
            Draw.RegenerateShapesModelVisual3ds();
            Draw.RedrawShapes();
        }
        private void DrawSphere(object sender, RoutedEventArgs e)
        {
            Point3D point = new Point3D(0, 0, 0);
            double diameter = 5;
            int resolution = 48;

            Draw.Shapes.AddSphere(point, diameter, resolution);
            Draw.Shapes.RecentShape.Color(Colors.Red);
            Draw.RegenerateShapesModelVisual3ds();
            Draw.RedrawShapes();
        }
        private void DrawPerformanceTest(object sender, RoutedEventArgs e)
        {
            for (int i = 1; i < 20; i++)
            {
                for (int j = 1; j < 20; j++)
                {
                    for (int k = 1; k < 20; k++)
                    {
                        Draw.Shapes.AddCylinderClosed(new Point3D(i, j, k), new Vector3D(0.5, 0, 0), 0.2, 16);
                        Draw.Shapes.RecentShape.Color(Colors.Magenta);
                    }
                }
            }
            Draw.RegenerateShapesModelVisual3ds();
            Draw.RedrawShapes();
        }

        internal void DrawCicle()
        {

        }
        internal void DrawCicleCenterRadius()
        {
            
        }

        private void AddMaterial(object sender, RoutedEventArgs e)
        {

        }
        private void AddSection(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 사용자의 마우스 위치에 따라 ObjectSnapPoint를 찾아서 표현하고 snapPoint를 반환.
        /// </summary>
        /// <param name="p0"></param>
        internal ObjectSnapPoint ChangeToSnapPointAndDrawMark(ref Point p0)
        {
            if (!Settings.Default.isOnObjectSnap)
            {
                SnapPoint = null;
                return null;
            }

            SnapPoint = draw.ChangeToSnapPoint(ref p0);

            draw2d.objectSnapMark.Draw(SnapPoint);
            return SnapPoint;
        }
        /// <summary>
        /// ObjectSnapeMark를 클릭하면 grdMain에서 이벤트가 발생하지 않음. OsnapMark에 따로 이벤트를 넣어줘야함.
        /// </summary>
        /// <param name="p0"></param>
        internal ObjectSnapPoint ChangeToSnapPointAndDrawMark(ref Point p0, MouseButtonEventHandler eventObject)
        {
            SnapPoint = ChangeToSnapPointAndDrawMark(ref p0);
            if (SnapPoint != null)
            {
                SnapPoint.PutEventAtMark(eventObject);
            }
            return SnapPoint;
        }
        internal ObjectSnapPoint SnapPoint { get; set; }

        private void SwitchObjectSnap(object sender, RoutedEventArgs e)
        {
            SetObjectSnap(!Settings.Default.isOnObjectSnap);
            cmd.GetCursor();
        }
        private void SetObjectSnap(bool isOn)
        {
            Settings.Default.isOnObjectSnap = isOn;
            if (isOn)
            {
                btnObjectSnap.Background = pressedButtonColor;
            }
            else
            {
                btnObjectSnap.Background = unpressedButtonColor;
                draw2d.objectSnapMark.Clear();
            }
        }

        private void SwitchOrthogonal(object sender, RoutedEventArgs e)
        {
            SetOrthogonalOption(!Settings.Default.isOnOrthogonal);
            MouseInputGuideShapes.orthogonal = !Settings.Default.isOnOrthogonal;
            cmd.GetCursor();
        }

        private void SetOrthogonalOption(bool isOn)
        {
            Settings.Default.isOnOrthogonal = isOn;
            if (isOn)
            {
                btnOrthogonal.Background = unpressedButtonColor;
            }
            else
            {
                btnOrthogonal.Background = pressedButtonColor;
            }
        }
        private SolidColorBrush pressedButtonColor = Brushes.Gray;
        private SolidColorBrush unpressedButtonColor = Brushes.LightGray;
        private void SwitchFreeOrbit(object sender, RoutedEventArgs e)
        {
            IsOnOrbit = !IsOnOrbit;
        }

    }
    partial class MainWindow : Window
    {
        internal void SelectNode()
        {
            requestUserInput = new RequestUserInput(this);
            requestUserInput.RequestInts(Properties.Resource.String2);
            requestUserInput.actionAfterIntsWithInts += Fem.SelectNode;
            requestUserInput.Start();
        }
        internal void SelectElem()
        {
            requestUserInput = new RequestUserInput(this);
            requestUserInput.RequestInts(Properties.Resource.SelectElementNumber);
            requestUserInput.actionAfterIntsWithInts += Fem.SelectElem;
            requestUserInput.Start();
        }
        private void SelectElemByFenceLine(Point3D p0, Point3D p1)
        {
            Point3D pos = Draw.PCamera.Position;
            Vector3D v0 = p0 - pos;
            Vector3D v1 = p1 - pos;

            //draw.GetInfiniteTriangleBySelectionFence(p0, p1, ref pos, ref v0, ref v1);
            Fem.SelectByInfiniteTriangle(pos, v0, v1);

            RedrawFemModel();
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
            Draw.GetInfinitePyramidBySelectionWindow(wP0, wP1, ref p0, ref v0, ref v1, ref v2, ref v3);
            if (wP0.X > wP1.X)
            {
                //사각형을 반대방향으로 그린 경우 경계선에 겹친 모든 요소를 선택함.
                Fem.SelectByInfinitePyramid_Cross(p0, v0, v1, v2, v3);
            }
            else
            {
                Fem.SelectByInfinitePyramid(p0, v0, v1, v2, v3);
            }
        }

        private void AddNode(object sender, RoutedEventArgs e)
        {
            requestUserInput = new RequestUserInput(this);
            requestUserInput.viewType = MouseInputGuideShapes.ViewType.Cross;
            requestUserInput.RequestPoints(-1);
            requestUserInput.actionAfterEveryLastPointWithPoint += Fem.Model.Nodes.Add_NoReturn;
            requestUserInput.actionAfterEveryPoint += RedrawFemModel;
            requestUserInput.Start();
        }

        private void AddFemLine(object sender, RoutedEventArgs e)
        {
            Cmd.Call("Line");
        }
        internal void AddFemLine()
        {
            requestUserInput = new RequestUserInput(this);
            requestUserInput.viewType = MouseInputGuideShapes.ViewType.Line;
            requestUserInput.RequestPoints(Properties.Resource.String7);
            requestUserInput.actionEveryLastTwoPointsWithPointPoint += AddFemLine;
            requestUserInput.actionEveryLastTwoPoints += RedrawFemModel;
            requestUserInput.Start();
        }
        internal void AddFemLine(Point3D p0, Point3D p1)
        {
            FemNode n1 = Fem.Model.Nodes.Add(p0);
            FemNode n2 = Fem.Model.Nodes.Add(p1);
            Fem.Model.Elems.AddFrame(n1, n2);
        }

        private void FemDivide(object sender, RoutedEventArgs e)
        {
            Cmd.Call("Divide");
        }
        internal void DivideElem()
        {
            requestUserInput = new RequestUserInput(this);
            requestUserInput.RequestElemSelection(Properties.Resource.String1);
            requestUserInput.RequestInt(Properties.Resource.String3);
            requestUserInput.actionAfterIntWithInt += Fem.DivideSelectedElems;
            requestUserInput.actionEnd += Fem.Selection.Clear;
            requestUserInput.actionEnd += RedrawFemModel;
            requestUserInput.Start();
        }
        private void FemExtrude(object sender, RoutedEventArgs e)
        {
            Cmd.Call("Extrude");
        }
        internal void ExtrudeElem()
        {
            requestUserInput = new RequestUserInput(this);
            requestUserInput.RequestElemSelection(Properties.Resource.String4);
            requestUserInput.RequestVector(Properties.Resource.String5);
            requestUserInput.RequestInt(Properties.Resource.String6);
            requestUserInput.actionAfterIntWithVecInt += Fem.ExtrudeWoReturn;
            requestUserInput.actionEnd += Fem.Selection.Clear;
            requestUserInput.actionEnd += RedrawFemModel;
            requestUserInput.Start();
        }

        internal void FemMoveSelected()
        {
            requestUserInput = new RequestUserInput(this);
            requestUserInput.RequestNodeSelection("이동할 절점을 선택하세요.");
            requestUserInput.RequestVector("이동시킬 방향을 설정하세요.");
            requestUserInput.actionAfterVecWithVec += Fem.MoveNode;
            requestUserInput.actionEnd += Fem.Selection.Clear;
            requestUserInput.actionEnd += RedrawFemModel;
            requestUserInput.Start();
        }

        internal void BoundaryFixRx()
        {
            requestUserInput = new RequestUserInput(this);
            requestUserInput.RequestNodeSelection(Properties.Resource.String10);
            boundaryFixFreeCondition = new int[] { 0, 0, 0, 1, 0, 0 };
            requestUserInput.actionEnd += BoundaryAdd_SelectedNode;
            requestUserInput.actionEnd += Fem.Selection.Clear;
            requestUserInput.actionEnd += RedrawFemModel;
            requestUserInput.Start();
        }
        internal void BoundaryFixRy()
        {
            requestUserInput = new RequestUserInput(this);
            requestUserInput.RequestNodeSelection(Properties.Resource.String10);
            boundaryFixFreeCondition = new int[] { 0, 0, 0, 0, 1, 0 };
            requestUserInput.actionEnd += BoundaryAdd_SelectedNode;
            requestUserInput.actionEnd += Fem.Selection.Clear;
            requestUserInput.actionEnd += RedrawFemModel;
            requestUserInput.Start();
        }
        internal void BoundaryRemove()
        {
            requestUserInput = new RequestUserInput(this);
            requestUserInput.RequestNodeSelection(Properties.Resource.String10);
            requestUserInput.actionEnd += BoundaryRemoveAll;
            requestUserInput.actionEnd += Fem.Selection.Clear;
            requestUserInput.actionEnd += RedrawFemModel;
            requestUserInput.Start();
        }
        private void BoundaryRemoveAll()
        {
        }
        internal void BoundaryFixRz()
        {
            requestUserInput = new RequestUserInput(this);
            requestUserInput.RequestNodeSelection(Properties.Resource.String10);
            boundaryFixFreeCondition = new int[] { 0, 0, 0, 0, 0, 1 };
            requestUserInput.actionEnd += BoundaryAdd_SelectedNode;
            requestUserInput.actionEnd += Fem.Selection.Clear;
            requestUserInput.actionEnd += RedrawFemModel;
            requestUserInput.Start();
        }
        internal void BoundaryFixDz()
        {
            requestUserInput = new RequestUserInput(this);
            requestUserInput.RequestNodeSelection(Properties.Resource.String10);
            boundaryFixFreeCondition = new int[] { 0, 0, 1, 0, 0, 0 };
            requestUserInput.actionEnd += BoundaryAdd_SelectedNode;
            requestUserInput.actionEnd += Fem.Selection.Clear;
            requestUserInput.actionEnd += RedrawFemModel;
            requestUserInput.Start();
        }
        internal void BoundaryFixDy()
        {
            requestUserInput = new RequestUserInput(this);
            requestUserInput.RequestNodeSelection(Properties.Resource.String10);
            boundaryFixFreeCondition = new int[] { 0, 1, 0, 0, 0, 0 };
            requestUserInput.actionEnd += BoundaryAdd_SelectedNode;
            requestUserInput.actionEnd += Fem.Selection.Clear;
            requestUserInput.actionEnd += RedrawFemModel;
            requestUserInput.Start();
        }
        internal void BoundaryFixDx()
        {
            requestUserInput = new RequestUserInput(this);
            requestUserInput.RequestNodeSelection(Properties.Resource.String10);
            boundaryFixFreeCondition = new int[] { 1, 0, 0, 0, 0, 0 };
            requestUserInput.actionEnd += BoundaryAdd_SelectedNode;
            requestUserInput.actionEnd += Fem.Selection.Clear;
            requestUserInput.actionEnd += RedrawFemModel;
            requestUserInput.Start();
        }
        internal void BoundaryFixDXYZ()
        {
            requestUserInput = new RequestUserInput(this);
            requestUserInput.RequestNodeSelection(Properties.Resource.String10);
            boundaryFixFreeCondition = new int[] { 1, 1, 1, 0, 0, 0 };
            requestUserInput.actionEnd += BoundaryAdd_SelectedNode;
            requestUserInput.actionEnd += Fem.Selection.Clear;
            requestUserInput.actionEnd += RedrawFemModel;
            requestUserInput.Start();
        }
        internal void BoundaryFixAll()
        {
            requestUserInput = new RequestUserInput(this);
            requestUserInput.RequestNodeSelection(Properties.Resource.String10);
            boundaryFixFreeCondition = new int[] { 1, 1, 1, 1, 1, 1 };
            requestUserInput.actionEnd += BoundaryAdd_SelectedNode;
            requestUserInput.actionEnd += Fem.Selection.Clear;
            requestUserInput.actionEnd += RedrawFemModel;
            requestUserInput.Start();
        }
        private int[] boundaryFixFreeCondition;
        private void BoundaryAdd_SelectedNode()
        {
            int dx = boundaryFixFreeCondition[0];
            int dy = boundaryFixFreeCondition[1];
            int dz = boundaryFixFreeCondition[2];
            int rx = boundaryFixFreeCondition[3];
            int ry = boundaryFixFreeCondition[4];
            int rz = boundaryFixFreeCondition[5];
            foreach (FemNode node in Fem.Selection.nodes)
            {
                Fem.Model.Boundaries.AddBoundary(node, dx, dy, dz, rx, ry, rz);
            }
        }

        private void FemAddLoadSelected(object sender, RoutedEventArgs e)
        {
            Cmd.Call("Force");
        }
        internal void FemAddLoadSelected()
        {
            requestUserInput = new RequestUserInput(this);
            requestUserInput.RequestNodeSelection("하중을 재하할 노드를 선택하세요.");
            requestUserInput.RequestVectorValue("하중의 크기를 지정하세요. (ex. 10,0,0)");
            requestUserInput.actionAfterVecWithVec += Fem.AddForceSelectedNodes;
            requestUserInput.actionEnd += Fem.Selection.Clear;
            requestUserInput.actionEnd += RedrawFemModel;
            requestUserInput.Start();
        }

        public void EraseAll()
        {
            Fem.DeleteAllNode();
            RedrawFemModel();
        }
        internal void EraseSelected()
        {
            Fem.DeleteSelection();
            RedrawFemModel();
        }
        internal void EraseFence()
        {
            requestUserInput = new RequestUserInput(this);
            requestUserInput.RequestPoints("define fence");
            requestUserInput.viewType = MouseInputGuideShapes.ViewType.Line;
            requestUserInput.actionEveryLastTwoPointsWithPointPoint += SelectElemByFenceLine;
            requestUserInput.actionEnd += EraseSelected;
            requestUserInput.Start();
        }

        private void FemAnalysis(object sender, RoutedEventArgs e)
        {
            FemAnalysis();
        }
        private void FemAnalysis()
        {
            Fem.Check();
            Fem.Solve();
            RedrawFemModel();
        }

    } // FEM
    partial class MainWindow : Window
    {
        private void OpenPannelCameraControl(object sender, RoutedEventArgs e)
        {
            //dkpCameraControl.Visibility = Visibility.Visible;
            AfterViewChanged();
        }
        private void ClosePannelCameraControl(object sender, RoutedEventArgs e)
        {
            //dkpCameraControl.Visibility = Visibility.Collapsed;
        }
        private void AfterViewChanged()
        {
            GetCameraInfo();
            RedrawShapesWo3dGeneration();
        }
        private void GetCameraInfo()
        {
            tbxCameraPositionX.Text = Draw.PCamera.Position.X.ToString();
            tbxCameraPositionY.Text = Draw.PCamera.Position.Y.ToString();
            tbxCameraPositionZ.Text = Draw.PCamera.Position.Z.ToString();
            tbxCameraLookDirectionX.Text = Draw.PCamera.LookDirection.X.ToString();
            tbxCameraLookDirectionY.Text = Draw.PCamera.LookDirection.Y.ToString();
            tbxCameraLookDirectionZ.Text = Draw.PCamera.LookDirection.Z.ToString();
            tbxCameraUpDirectionX.Text = Draw.PCamera.UpDirection.X.ToString();
            tbxCameraUpDirectionY.Text = Draw.PCamera.UpDirection.Y.ToString();
            tbxCameraUpDirectionZ.Text = Draw.PCamera.UpDirection.Z.ToString();
            tbxCameraFarPlaneDistance.Text = Draw.PCamera.FarPlaneDistance.ToString();
            tbxCameraFieldOfView.Text = Draw.PCamera.FieldOfView.ToString();
            tbxCameraNearPlaneDistance.Text = Draw.PCamera.NearPlaneDistance.ToString();
        }
        private void SetCameraInfo(object sender, RoutedEventArgs e)
        {
            Draw.PCamera.Position = new Point3D
            {
                X = Convert.ToDouble(tbxCameraPositionX.Text),
                Y = Convert.ToDouble(tbxCameraPositionY.Text),
                Z = Convert.ToDouble(tbxCameraPositionZ.Text)
            };
            Draw.PCamera.LookDirection = new Vector3D
            {
                X = Convert.ToDouble(tbxCameraLookDirectionX.Text),
                Y = Convert.ToDouble(tbxCameraLookDirectionY.Text),
                Z = Convert.ToDouble(tbxCameraLookDirectionZ.Text)
            };
            Draw.PCamera.UpDirection = new Vector3D
            {
                X = Convert.ToDouble(tbxCameraUpDirectionX.Text),
                Y = Convert.ToDouble(tbxCameraUpDirectionY.Text),
                Z = Convert.ToDouble(tbxCameraUpDirectionZ.Text)
            };
            Draw.PCamera.FieldOfView = Convert.ToDouble(tbxCameraFieldOfView.Text);
            Draw.PCamera.NearPlaneDistance = Convert.ToDouble(tbxCameraNearPlaneDistance.Text);
            Draw.PCamera.FarPlaneDistance = Convert.ToDouble(tbxCameraFarPlaneDistance.Text);
        }

        private void OpenPannelConcreteSetting(object sender, RoutedEventArgs e)
        {
            //dkpStructureConcrete.Visibility = Visibility.Visible;
            tbxHeight.Text = smr.structure.height.ToString();
            tbxLength.Text = smr.structure.length.ToString();
            tbxWidth.Text = smr.structure.width.ToString();
        }
        private void ClosePannelConcreteSetting(object sender, RoutedEventArgs e)
        {
            //dkpStructureConcrete.Visibility = Visibility.Collapsed;
        }
        private void SetConcrete(object sender, RoutedEventArgs e)
        {
            smr.structure.height = Convert.ToDouble(tbxHeight.Text);
            smr.structure.length = Convert.ToDouble(tbxLength.Text);
            smr.structure.width = Convert.ToDouble(tbxWidth.Text);
            Draw.Shapes.AddBox(new Point3D(0, 0, 0), new Vector3D(smr.structure.length, smr.structure.width, smr.structure.height));
            Draw.RegenerateShapesModelVisual3ds();
            Draw.RedrawShapes();

    
        }

        private void OpenPannelFemWorks(object sender, RoutedEventArgs e)
        {
            //dkpFemWorks.Visibility = Visibility.Visible;
            RedrawFemWorksTreeView();
        }
        private void ClosePannelFemWorks(object sender, RoutedEventArgs e)
        {
            //dkpFemWorks.Visibility = Visibility.Collapsed;
        }
        private void RedrawFemWorksTreeView()
        {
            //ref. https://www.codeproject.com/Articles/124644/Basic-Understanding-of-Tree-View-in-WPF
            treeViewFemWorks.Items.Clear();

            TreeViewItem item = new TreeViewItem();
            item.Header = "Materials(" + Fem.Model.Materials.Count + ")";
            item.IsExpanded = false;
            foreach (FemMaterial material in Fem.Model.Materials)
            {
                item.Items.Add(new TreeViewItem() { Header = material.name });
            }
            treeViewFemWorks.Items.Add(item);

            item = new TreeViewItem();
            item.Header = "Sections(" + Fem.Model.Sections.Count + ")";
            item.IsExpanded = false;
            foreach (FemSection section in Fem.Model.Sections)
            {
                item.Items.Add(new TreeViewItem() { Header = section.num });
            }
            treeViewFemWorks.Items.Add(item);

            item = new TreeViewItem();
            item.Header = "Nodes(" + Fem.Model.Nodes.Count + ")";
            item.IsExpanded = true;
            treeViewFemWorks.Items.Add(item);

            item = new TreeViewItem();
            FemElementCollection elems = Fem.Model.Elems;
            item.Header = "Elements(" + elems.Count + ")";
            item.IsExpanded = true;
            if (elems.Frames.Count != 0) item.Items.Add(new TreeViewItem() { Header = "Frame(" + elems.Frames.Count + ")" });
            if (elems.Plates.Count != 0) item.Items.Add(new TreeViewItem() { Header = "Plate(" + elems.Plates.Count + ")" });
            if (elems.Solids.Count != 0) item.Items.Add(new TreeViewItem() { Header = "Solid(" + elems.Solids.Count + ")" });
            treeViewFemWorks.Items.Add(item);

            item = new TreeViewItem();
            item.Header = "Boundaries(" + Fem.Model.Boundaries.Count + ")";
            item.IsExpanded = false;
            foreach (FemBoundary boundary in Fem.Model.Boundaries)
            {
                item.Items.Add(new TreeViewItem() { Header = boundary.node.Num });
            }
            treeViewFemWorks.Items.Add(item);

            item = new TreeViewItem();
            item.Header = "Loads(" + Fem.Loads.Count + ")";
            item.IsExpanded = false;
            foreach (FemBoundary boundary in Fem.Model.Boundaries)
            {
                item.Items.Add(new TreeViewItem() { Header = boundary.node.Num });
            }
            treeViewFemWorks.Items.Add(item);
        }

        private void ViewCoordinateSystem(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem sd = (System.Windows.Controls.MenuItem)sender;
            Draw.ShowCoordinateSystem = sd.IsChecked;
            Draw.RegenerateShapesModelVisual3ds();
            Draw.RedrawShapes();
        }
        private void ViewBasePlaneGrid(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem sd = (System.Windows.Controls.MenuItem)sender;
            Draw.ShowBasePlaneGrid = sd.IsChecked;
            Draw.RegenerateShapesModelVisual3ds();
            Draw.RedrawShapes();
        }

        private void RedrawFemModel(object sender, RoutedEventArgs e)
        {
            cmd.Call("Regen");
        }
        public void RedrawFemModel()
        {
            RedrawFemWorksTreeView();

            bool showUndeformedShape = true;
            bool showSection = true;

            double loadViewScale = GetLoadViewScale();

            Draw.Shapes.Clear();
            Draw.Texts.Clear();

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
            if (Fem.solved)
            {
                if (Fem.Model.Nodes.visibility)
                {
                    foreach (FemNode node in Fem.Model.Nodes)
                    {
                        Draw.Shapes.AddSphere(node.C1, diaNode, rlsNode);
                        Draw.Shapes.RecentShape.Color(colorNode);
                    }
                }
                if (Fem.Model.Elems.show)
                {
                    foreach (FemElement e in Fem.Model.Elems)
                    {
                        switch (e.type)
                        {
                            case 21:
                                FemFrame frame = (FemFrame)e;
                                Point3D str = frame.nodes[0].C1;
                                Point3D end = frame.nodes[1].C1;
                                Vector3D dir = end - str;
                                if (showSection | frame.Section == null)
                                {
                                    Draw.Shapes.AddPolygon(str, dir, frame.Section.Poly);
                                }
                                else
                                {
                                    Draw.Shapes.AddLine(str, str + dir);
                                }
                                break;
                            case 40:
                                FemPlate p = (FemPlate)e;
                                Draw.Shapes.AddBox(p.nodes[0].C1, p.nodes[2].C1 - p.nodes[0].C1);

                                break;
                            case 80:
                                FemSolid s = (FemSolid)e;
                                Draw.Shapes.AddHexahedron(s.nodes[0].C1, s.nodes[1].C1, s.nodes[2].C1, s.nodes[3].C1, s.nodes[4].C1, s.nodes[5].C1, s.nodes[6].C1, s.nodes[7].C1);
                                break;
                            default:
                                Draw.Shapes.RecentShape.Color(colorElem);
                                break;
                        }
                    }
                }
                foreach (FemLoad load in Fem.Loads)
                {
                    foreach (FemNodalLoad nodalLoad in load.nodalLoads)
                    {
                        Draw.Shapes.AddForce(nodalLoad.node.C1, nodalLoad.force * loadViewScale);
                        Draw.Shapes.RecentShape.Color(colorLoad);
                    }
                }

                //Reaction Force
                foreach (FemNode node in Fem.Model.Nodes)
                {
                    Vector3D dir = new Vector3D(node.reactionForce[0], node.reactionForce[1], node.reactionForce[2]);
                    Draw.Shapes.AddForce(node.C1, dir * loadViewScale);
                    Draw.Shapes.RecentShape.Color(colorReaction);
                }
            }

            //Undeformed
            if (showUndeformedShape)
            {
                bool showUndeformedForce;
                double opacity;

                if (Fem.solved)
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

                if (Fem.Model.Elems.show)
                {
                    foreach (FemElement e in Fem.Model.Elems)
                    {
                        switch (e.type)
                        {
                            case 21:
                                FemFrame frame = (FemFrame)e;
                                Point3D str = frame.nodes[0].C0;
                                Point3D end = frame.nodes[1].C0;
                                Vector3D dir = end - str;

                                if (frame.Section == null)
                                {
                                    //Draw.Shapes.AddCylinder(str, dir, diaElem, rlsElem);
                                    Draw.Shapes.AddLine(str, str + dir);
                                }
                                else
                                {
                                    if (showSection & frame.Section.hasSectionPoly)
                                    {
                                        Draw.Shapes.AddPolygon(str, dir, frame.Section.Poly);
                                    }
                                    else
                                    {
                                        //Draw.Shapes.AddCylinder(str, dir, diaElem, rlsElem);
                                        Draw.Shapes.AddLine(str, str + dir);
                                    }
                                }
                                break;
                            case 40:
                                FemPlate p = (FemPlate)e;
                                //draw.shapes.AddBox(p.nodes[0].c0, p.nodes[2].c0 - p.nodes[0].c0);
                                Draw.Shapes.AddRectangle(p.nodes[0].C0, p.nodes[1].C0, p.nodes[2].C0, p.nodes[3].C0);
                                break;
                            case 80:
                                FemSolid s = (FemSolid)e;
                                Draw.Shapes.AddHexahedron(s.nodes[0].C0, s.nodes[1].C0, s.nodes[2].C0, s.nodes[3].C0, s.nodes[4].C0, s.nodes[5].C0, s.nodes[6].C0, s.nodes[7].C0);
                                break;
                            default:
                                break;
                        }
                        if (e.selected)
                        {
                            Draw.Shapes.RecentShape.Color(colorSelectedElem);
                        }
                        else
                        {
                            Draw.Shapes.RecentShape.Color(colorElem);
                        }
                        Draw.Shapes.RecentShape.Opacity(opacity);
                    }
                }
                if (!Fem.solved)
                {
                    if (Fem.Model.Nodes.visibility)
                    {
                        foreach (FemNode node in Fem.Model.Nodes)
                        {
                            Draw.Shapes.AddSphere(node.C0, diaNode, rlsNode);
                            if (node.selected)
                            {
                                Draw.Shapes.RecentShape.Color(colorSelectedNode);
                            }
                            else
                            {
                                Draw.Shapes.RecentShape.Color(colorNode);
                            }
                            Draw.Shapes.RecentShape.Opacity(opacity);
                        }
                    }
                }
                if (showUndeformedForce)
                {
                    foreach (FemLoad load in Fem.Loads)
                    {
                        foreach (FemNodalLoad nodalLoad in load.nodalLoads)
                        {
                            Draw.Shapes.AddForce(load.nodalLoads[0].node.C0, nodalLoad.force * loadViewScale);
                            Draw.Shapes.RecentShape.Color(colorLoad);
                            Draw.Shapes.RecentShape.Opacity(opacity);
                        }
                    }
                }
            }

            //나중에 추가한 개체가 앞에 표시됨.
            RedrawShapes();
        }

        private double GetLoadViewScale()
        {
            double maxLoadLength = 2;
            double maxLoadSize = Fem.Loads.GetMaxLoadSize();
            double loadViewScale = maxLoadLength / maxLoadSize;
            return loadViewScale;
        }

        public void RedrawShapes(object sender, RoutedEventArgs e)
        {
            cmd.Call("Redraw");
        }
        public void RedrawShapes()
        {
            Draw.RegenerateShapesModelVisual3ds();
            RedrawShapesWo3dGeneration();
        }
        public void RedrawShapesWo3dGeneration()
        {
            Draw.RedrawShapes();
            Redraw3dRelated2dShapes();
            draw2d.RedrawShapes();
        }

        private void Redraw3dRelated2dShapes()
        {
            if (grdMain.ActualHeight == 0) return;
            if (Fem.Model.Boundaries.visibility)
            {
                draw2d.boundaryConditionMarks.Clear();
                foreach (FemBoundary boundary in Fem.Model.Boundaries)
                {
                    Point3D p0 = boundary.node.C0;
                    Point p = Draw.GetPoint2DFromPoint3D(p0);
                    draw2d.boundaryConditionMarks.Add(p, boundary.condition);
                }
            }

            draw2d.texts.Clear();
            if (Settings.Default.isFemViewNodeNumber)
            {
                foreach (FemNode node in Fem.Model.Nodes)
                {
                    Point3D p0 = node.C1;
                    Point p = Draw.GetPoint2DFromPoint3D(p0);
                    Text2D t = draw2d.texts.Add(p, node.Num.ToString(cultureInfo));
                    t.Color = Brushes.Brown;
                }
            }
            if (Settings.Default.isFemViewElemNumber)
            {
                foreach (FemElement element in Fem.Model.Elems)
                {
                    Point3D p0 = element.CenterC1;
                    Point p = draw.GetPoint2DFromPoint3D(p0);
                    Text2D t = draw2d.texts.Add(p, element.Num.ToString(cultureInfo),Settings.Default.nodeNumberSize);
                    t.Color = Brushes.Blue;
                    t.Allignment = Text2D.Allignments.bottomCenter;
                }
            }

            double loadViewScale = GetLoadViewScale();
            foreach (FemLoad load in fem.Loads)
            {
                foreach (FemNodalLoad nodalLoad in load.nodalLoads)
                {
                    Point3D p0 = nodalLoad.node.C1 - nodalLoad.force * loadViewScale;
                    Point p = draw.GetPoint2DFromPoint3D(p0);
                    Text2D t = draw2d.texts.Add(p, nodalLoad.force.Length.ToString(cultureInfo)
                        + " " + Settings.Default.forceUnit, Settings.Default.nodeNumberSize);
                    t.Color = Brushes.Red;
                    t.Allignment = Text2D.Allignments.middleCenter;
                }
            }

        }
    } // 패널, 격자배경, 좌표계, Redraw 등
    partial class MainWindow : Window
    {
        private Point pointMouseDown;

        internal bool IsOnWindowSelect
        {
            get
            {
                return isOnWindowSelect;
            }
            set
            {
                if (value)
                {
                    if (isOnWindowSelect) return; //이벤트 중복생성 방지
                    MouseDown += WindowSelection_MouseLeftDown;
                }
                else
                {
                    MouseDown -= WindowSelection_MouseLeftDown;
                }
                isOnWindowSelect = value;
            }
        }
        private bool isOnWindowSelect = false;
        internal bool IsOnDeselectAll_Esc
        {
            get
            {
                return IsOnDeselectAll_Esc;
            }
            set
            {
                if (value)
                {
                    if (isOnDeselectAll_Esc) return;
                    KeyDown += UnselectAll_OrbitEnd_Esc;
                }
                else
                {
                    KeyDown -= UnselectAll_OrbitEnd_Esc;
                }
                isOnDeselectAll_Esc = value;
            }
        }
        private bool isOnDeselectAll_Esc = false;
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
        private void TurnOnFemAnalysis_F5(bool on)
        {
            if (on)
            {
                PreviewKeyDown += FemAnalysis_F5;
            }
            else
            {
                PreviewKeyDown -= FemAnalysis_F5;
            }
        }
        private void WindowSelection_MouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                RequestUserMouseWindowInput r = new RequestUserMouseWindowInput(this);
                r.viewType = MouseInputGuideShapes.ViewType.SelectionWindow;
                r.FirstPoint = e.GetPosition(grdMain);
                r.action = SelectFemByWindow;
                r.Start();
            }
        }
        private void UnselectAll_OrbitEnd_Esc(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                cmd.GetCursor();
                if (IsOnOrbit)
                {
                    IsOnOrbit = false;
                    return;
                }

                Fem.Selection.DeselectAll();
                RedrawFemModel();
            }
        }
        private void Erase_Del(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                Fem.Selection.Delete();
                RedrawFemModel();
            }
        }
        private void FemAnalysis_F5(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                FemAnalysis();
            }
        }
    } // 마우스 이벤트 관련
    partial class MainWindow : Window
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
            Draw.RegenerateShapesModelVisual3ds();
            Draw.RedrawShapes();
        }

        private void ViewSW(object sender, RoutedEventArgs e)
        {
            Draw.ViewSW();
            AfterViewChanged();
        }
        private void ViewSE(object sender, RoutedEventArgs e)
        {
            Draw.ViewSE();
            AfterViewChanged();
        }
        private void ViewNW(object sender, RoutedEventArgs e)
        {
            Draw.ViewNW();
            AfterViewChanged();
        }
        private void ViewNE(object sender, RoutedEventArgs e)
        {
            Draw.ViewNE();
            AfterViewChanged();
        }
        private void ViewTop(object sender, RoutedEventArgs e)
        {
            Draw.ViewTop();
            AfterViewChanged();
        }
        private void ViewFront(object sender, RoutedEventArgs e)
        {
            Draw.ViewFront();
            AfterViewChanged();
        }
        private void ViewBottom(object sender, RoutedEventArgs e)
        {
            Draw.ViewBottom();
            AfterViewChanged();
        }
        private void ViewLeft(object sender, RoutedEventArgs e)
        {
            Draw.ViewLeft();
            AfterViewChanged();
        }
        private void ViewRight(object sender, RoutedEventArgs e)
        {
            Draw.ViewRight();
            AfterViewChanged();
        }
        private void ViewBack(object sender, RoutedEventArgs e)
        {
            Draw.ViewBack();
            AfterViewChanged();
        }

        public void ZoomExtents(object sender, RoutedEventArgs e)
        {
            Draw.ViewZoomExtend();
            AfterViewChanged();
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
            requestUserInput.viewType = MouseInputGuideShapes.ViewType.Rectangle;
            requestUserInput.RequestPoints(2);
            requestUserInput.actionEveryLastTwoPointsWithPointPoint += Draw.ViewZoomWindow;
            requestUserInput.actionEnd += AfterViewChanged;
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
            Zoom_Forward(e.Delta);
        }
        private void Zoom_ForwardOneStep(object sender, RoutedEventArgs e)
        {
            Zoom_Forward(500);
        }
        private void Zoom_ForwardBackStep(object sender, RoutedEventArgs e)
        {
            Zoom_Forward(-500);
        }
        internal void Zoom_Forward(int zoomInt)
        {
            Draw.ZoomForward(zoomInt);
            AfterViewChanged();
        }
        private void PanOn_MouseWheelDown(object sender, MouseButtonEventArgs e)
        {
            if(e.MiddleButton == MouseButtonState.Pressed)
            {
                pointMouseDown = e.GetPosition(grdMain);
                Draw.OrbitStart();
                if (requestUserInput.IsOn)
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
                Draw.OrbitMove(mov);
                if (requestUserInput.IsOn)
                {
                    //requestUserInput.PanMove(mov);
                }
                AfterViewChanged();
            }
        }
        private void PanOff_MouseWheelUp(object sender, MouseButtonEventArgs e)
        {
            Draw.OrbitEnd();
            if (requestUserInput.IsOn)
            {
                //requestUserInput.PanMoveEnd();
            }
            MouseMove -= Pan_MouseWheelDownMove;
        }

        private void TurnOnOrbit(object sender, RoutedEventArgs e)
        {
            IsOnOrbit = true;
        }
        internal bool IsOnOrbit
        {
            get
            {
                return isOnOrbit;
            }
            set
            {
                if (value)
                {
                    if (isOnOrbit) return;
                    MouseDown += new MouseButtonEventHandler(OrbitStart_MouseDown);
                    MouseUp += new MouseButtonEventHandler(OrbitEnd_MouseUp);
                    grdMain.MouseLeave += new System.Windows.Input.MouseEventHandler(Orbit_MouseLeave);
                    KeyDown += TurnOffOrbit_Esc;
                }
                else
                {
                    MouseDown -= new MouseButtonEventHandler(OrbitStart_MouseDown);
                    MouseUp -= new MouseButtonEventHandler(OrbitEnd_MouseUp);
                    grdMain.MouseLeave -= new System.Windows.Input.MouseEventHandler(Orbit_MouseLeave);
                    MouseMove -= new System.Windows.Input.MouseEventHandler(Orbit_MouseMove);

                    KeyDown -= TurnOffOrbit_Esc;
                    IsOnWindowSelect = true;
                }

                if (value)
                {
                    btnOrbit.Background = pressedButtonColor;
                }
                else
                {
                    btnOrbit.Background = unpressedButtonColor;
                }

                isOnOrbit = value;
            }
        }
        private bool isOnOrbit = false;
        private void OrbitStart_MouseDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            pointMouseDown = e.GetPosition(grdMain);
            Draw.OrbitStart();
            stbLabel.Content = "0, 0";
            MouseMove += new System.Windows.Input.MouseEventHandler(Orbit_MouseMove);
        }
        private void Orbit_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                isOnOrbit = true;
                Point p = e.GetPosition(grdMain);
                Vector mov = p - pointMouseDown;
                stbLabel.Content = mov.X + ", " + mov.Y;

                Draw.OrbitRotate(mov);
                AfterViewChanged();
            }
            else
            {
                //Point p = e.GetPosition(grdMain);
                //stbLabel.Content = (p.X) + ", " + (p.Y);
            }
            if (e.RightButton == MouseButtonState.Pressed)
            {
                isOnOrbit = true;
                //Point strPoint_grdMain = grdMain.TransformToAncestor(baseDockPanel).Transform(new Point(0, 0));
                Point strPoint_grdMain = new Point(0, 0);
                Point center_gridMain = new Point(strPoint_grdMain.X + grdMain.ActualWidth / 2, strPoint_grdMain.Y + grdMain.ActualHeight / 2);
                Point p = e.GetPosition(grdMain);

                //bckD.DrawLine(strPoint_grdMain, center_gridMain);

                Vector center2mouseDownPoint;
                center2mouseDownPoint = pointMouseDown - center_gridMain;
                //bckD.DrawLine(center_gridMain, pointMouseDown);

                Vector center2point = p - center_gridMain;

                double iniAngle = -Math.Atan2(center2mouseDownPoint.Y, center2mouseDownPoint.X);
                double newAngle = -Math.Atan2(center2point.Y, center2point.X);

                double rad = newAngle - iniAngle;

                stbLabel.Content = "rad = " + rad;
                Draw.OrbitTwist(rad);
                AfterViewChanged();
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
            Draw.OrbitEnd();
            this.MouseMove -= new System.Windows.Input.MouseEventHandler(Orbit_MouseMove);
            Draw.RedrawShapes();
            RedrawShapesWo3dGeneration();

        }
        private void TurnOffOrbit_Esc(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                IsOnOrbit = false;
            }
        }

        private void ViewNode(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem sd = (System.Windows.Controls.MenuItem)sender;
            Fem.Model.Nodes.visibility = sd.IsChecked;
            RedrawFemModel();
        }
        private void ViewNodeNumber(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem sd = (System.Windows.Controls.MenuItem)sender;
            Settings.Default.isFemViewNodeNumber = sd.IsChecked;
            Settings.Default.Save();
            RedrawFemModel();
        }
        private void ViewElement(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem sd = (System.Windows.Controls.MenuItem)sender;
            Fem.Model.Elems.show = sd.IsChecked;
            RedrawFemModel();
        }
        private void ViewElementNumber(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem sd = (System.Windows.Controls.MenuItem)sender;
            Settings.Default.isFemViewElemNumber = sd.IsChecked;
            Settings.Default.Save();
            RedrawShapesWo3dGeneration();
        }
    } // View 관련
}