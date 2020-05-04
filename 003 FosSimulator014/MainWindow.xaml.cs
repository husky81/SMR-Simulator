using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        internal readonly FEM fem;
        public readonly DRAW draw;
        public readonly CommandWindow cmd;
        internal readonly RequestUserCoordinatesInput requestUserCoordinatesInput;
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
            requestUserCoordinatesInput = new RequestUserCoordinatesInput(this);

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

        internal void AddLine()
        {
            requestUserCoordinatesInput.Reset(-1);
            requestUserCoordinatesInput.actionEveryLastTwoPoints = AddLineFem;
            requestUserCoordinatesInput.viewType = DRAW.SelectionWindow.ViewType.Line;
            requestUserCoordinatesInput.Start();
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
            requestUserCoordinatesInput.Reset(2);
            requestUserCoordinatesInput.viewType = DRAW.SelectionWindow.ViewType.Line;
            requestUserCoordinatesInput.actionEveryLastTwoPoints += SelectElemByFenceLine;
            requestUserCoordinatesInput.actionEnd += EraseSelected;
            requestUserCoordinatesInput.Start();
        }
        private void SelectElemByFenceLine(Point p0, Point p1)
        {
            Point3D pos = new Point3D();
            Vector3D v0 = new Vector3D(), v1 = new Vector3D();

            draw.GetInfiniteTriangleBySelectionFence(p0, p1, ref pos, ref v0, ref v1);
            fem.SelectByInfiniteTriangle(pos, v0, v1);

            RedrawFemModel();
        }

        private void FemDivide(object sender, RoutedEventArgs e)
        {
            cmd.CallCommand("Divide");
        }
        internal void DivideElem()
        {
            requestUserInput = new RequestUserInput(this);
            requestUserInput.RequestElemSelection("분할할 개체를 선택하세요.");
            requestUserInput.RequestInt("몇개로 나눌까요?");
            requestUserInput.actionAfterIntWithInt += fem.Divide;
            requestUserInput.Start();

            //if (fem.selection.elems.Count == 0)
            //{
            //    Elements elems = RequestSelectElement("분할할 개체를 선택하세요.");
            //    if (elems.Count == 0)
            //    {
            //        cmd.sendMessage("선택된 개체가 없습니다.");
            //        cmd.NewLine();
            //        return;
            //    }
            //}
            //int dividedNumber = RequestInt("몇개로 나눌까요?");
            //fem.Divide(dividedNumber);
        }
        private void FemExtrude(object sender, RoutedEventArgs e)
        {
            Vector3D dir = new Vector3D(0, 1, 0);
            Elements ee = fem.Extrude(dir, 2);
            RedrawFemModel();
        }
        internal void ExtrudeElem()
        {
            requestUserInput = new RequestUserInput(this);
            requestUserInput.RequestElemSelection("연장시킬 개체를 선택하세요.");
            requestUserInput.RequestDirection("연장시킬 방향을 지정하세요.");
            requestUserInput.RequestInt("몇번 반복할까요?");
            requestUserInput.actionAfterIntWithDirInt += fem.ExtrudeWoRetern;
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
            Double,
            Distance,
            Direction,
        }
        internal RequestInputType requestInputType;
        internal string message;
        internal bool hasAction;
    }
    public class RequestUserInput
    {
        private MainWindow main;
        public RequestUserInput(MainWindow main)
        {
            this.main = main;
        }

        private bool on = false;
        internal bool On
        {
            get
            {
                return on;
            }
            set
            {
                if (value != on)
                {
                    TurnOnMainWindowEvents(on);
                    main.KeyDown += ExitCommand_EscKey;
                }
                on = value;
            }
        }
        internal void End()
        {
            On = false;
            TurnOffAllEvents();
            RedrawFemModel();
            main.cmd.NewLine();
        }
        internal void End_Cancle()
        {
            On = false;
            TurnOffAllEvents();
            RedrawFemModel();
        }

        private void TurnOffAllEvents()
        {
            main.KeyDown -= ExitCommand_EscKey;

            main.MouseDown -= GetDirection;
            main.MouseMove -= GetDirection_Moving;
            main.MouseDown -= GetDirection_SecondPoint;
        }

        private void ExitCommand_EscKey(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                End_Cancle();
            }
        }

        internal UserInputAction.RequestInputType doingActionType;

        private void TurnOnMainWindowEvents(bool on)
        {
            main.WindowSelectionOn(on);
            main.TurnOnDeselectAll_Esc(on);
        }


        internal delegate void inputInt(int n);
        internal inputInt actionAfterIntWithInt;
        internal delegate void inputDirInt(Vector3D dir, int n);
        internal inputDirInt actionAfterIntWithDirInt;

        private UserInputAction LastAction
        {
            get
            {
                return userInputActions[userInputActions.Count - 1];
            }
        }
        private readonly List<UserInputAction> userInputActions = new List<UserInputAction>();
        private int actionStep = 0;

        private Vector3D userInputVector;
        private double userInputDouble;

        internal void RequestInt(string message)
        {
            UserInputAction userInputAction = new UserInputAction
            {
                requestInputType = UserInputAction.RequestInputType.Int,
                message = message
            };
            userInputActions.Add(userInputAction);
        }
        internal void RequestDirection(string message)
        {
            UserInputAction userInputAction = new UserInputAction
            {
                requestInputType = UserInputAction.RequestInputType.Direction,
                message = message
            };
            userInputActions.Add(userInputAction);
        }
        internal void RequestElemSelection(string message)
        {
            UserInputAction userInputAction = new UserInputAction
            {
                requestInputType = UserInputAction.RequestInputType.ElemSelection,
                message = message
            };
            userInputActions.Add(userInputAction);
        }

        internal void Start()
        {
            actionStep = -1;
            On = true;
            NextAction();
        }
        internal void DoAction()
        {
            if (actionStep >= userInputActions.Count)
            {
                End();
                return;
            }
            UserInputAction userInputAction = userInputActions[actionStep];
            doingActionType = userInputAction.requestInputType;
            switch (userInputAction.requestInputType)
            {
                case UserInputAction.RequestInputType.Point:
                    break;
                case UserInputAction.RequestInputType.TwoPoints:
                    break;
                case UserInputAction.RequestInputType.Points:
                    break;
                case UserInputAction.RequestInputType.ElemSelection:
                    if (main.fem.selection.elems.Count == 0)
                    {
                        main.cmd.SendRequestMessage(userInputAction.message);
                        End();
                    }
                    else
                    {
                        NextAction();
                    }
                    return;
                case UserInputAction.RequestInputType.NodeSelection:
                    break;
                case UserInputAction.RequestInputType.Selection:
                    break;
                case UserInputAction.RequestInputType.Int:
                    main.cmd.SendRequestMessage(userInputAction.message);
                    return;
                case UserInputAction.RequestInputType.Double:
                    break;
                case UserInputAction.RequestInputType.Distance:
                    break;
                case UserInputAction.RequestInputType.Direction:
                    main.cmd.SendRequestMessage(userInputAction.message);
                    main.MouseDown += GetDirection;
                    return;
                default:
                    break;
            }
            main.cmd.ErrorMessage("NextAction이 정의되지 않았습니다.");
            End();
            return;
        }

        private void GetDirection(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                Point p = e.GetPosition(main.grdMain);
                Point3D p3 = GetPoint3dFromPoint2D(p);
                main.MouseDown -= GetDirection;
                main.cmd.CallCommand(p3.X + "," + p3.Y + "," + p3.Z);
            }
        }
        private Point3D directionFirstPoint;
        internal void PutDirectionFirstPoint(Point3D userInputPoint3D)
        {
            directionFirstPoint = userInputPoint3D;
            Point p = GetPointFromPoint3D(userInputPoint3D);

            main.MouseMove += GetDirection_Moving;
            main.draw.selectionWindow.viewType = DRAW.SelectionWindow.ViewType.Line;
            main.draw.selectionWindow.Start(p);

            main.MouseDown += GetDirection_SecondPoint;
        }
        private void GetDirection_Moving(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Point p0 = e.GetPosition(main.grdMain);
            main.draw.selectionWindow.Move(p0);
        }
        private void GetDirection_SecondPoint(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point p = e.GetPosition(main.grdMain);
                Point3D p3 = GetPoint3dFromPoint2D(p);

                main.MouseDown -= GetDirection_SecondPoint;
                main.MouseMove -= GetDirection_Moving;
                main.draw.selectionWindow.End();

                Vector3D inputDirection = p3 - directionFirstPoint;
                Put(inputDirection);
            }
        }

        private void NextAction()
        {
            actionStep += 1;
            DoAction();
        }
        internal void Put(int userInputInt)
        {
            actionAfterIntWithInt?.Invoke(userInputInt);
            actionAfterIntWithDirInt?.Invoke(userInputVector, userInputInt);
            NextAction();
        }
        internal void Put(double userInputDouble)
        {
            this.userInputDouble = userInputDouble;
            NextAction();
        }
        internal void Put(Vector3D userInputVector)
        {
            this.userInputVector = userInputVector;
            NextAction();
        }

        private void RedrawFemModel()
        {
            main.RedrawFemModel();
        }
        private Point3D GetPoint3dFromPoint2D(Point p0)
        {
            return main.draw.GetPoint3dOnBasePlane_FromPoint2D(p0);
        }
        private Point GetPointFromPoint3D(Point3D p3d)
        {
            return main.draw.GetPoint2D_FromPoint3D(p3d);
        }


    }
    public class RequestUserCoordinatesInput
    {
        private readonly MainWindow main;
        public bool on = false;
        public int numRequestPoint = 1;
        private List<Point> points = new List<Point>();
        private List<Point3D> point3Ds = new List<Point3D>();
        /// <summary>
        /// 사용자가 마우스를 움직이면 보여지는 입력 모양을 선택합니다.
        /// </summary>
        internal DRAW.SelectionWindow.ViewType viewType;

        /// <summary>
        /// 매 입력마다 실행할 액션을 지정합니다.
        /// 마지막에 입력한 절점을 넘겨줍니다.
        /// Point p0
        /// </summary>
        internal ActionOnePoint actionEveryPoint;
        internal delegate void ActionOnePoint(Point p0);
        /// <summary>
        /// 첫 포인트를 제외하고 매 입력마다 실행할 Action을 지정합니다.
        /// 마지막에 입력한 절점 두개를 넘겨줍니다.
        /// Point p0, Point p1
        /// </summary>
        internal ActionTwoPoint actionEveryLastTwoPoints;
        internal delegate void ActionTwoPoint(Point p0, Point p1);
        /// <summary>
        /// 사용자 입력이 끝났을 때 실행할 Action을 지정합니다.
        /// 포인트리스트를 넘겨줍니다.
        /// List<Point> Plist
        /// </summary>
        internal ActionPointList actionPointInputEnd;
        internal delegate void ActionPointList(List<Point> Plist);
        /// <summary>
        /// 사용자 입력이 끝났을 때 실행할 Action을 지정합니다.
        /// 넘겨주는 값이 없습니다.
        /// 입력값이 없는 함수를 지정해야 합니다.
        /// </summary>
        internal ActionWithNone actionEnd;
        internal delegate void ActionWithNone();

        public RequestUserCoordinatesInput(MainWindow main)
        {
            this.main = main;
        }
        /// <summary>
        /// RequestUserCoordinatesInput을 초기화합니다. 시작하기 전에 무조건 실행해야 함.
        /// </summary>
        /// <param name="numRequestPoint">사용자한테 몇개 입력 받을까?</param>
        internal void Reset(int numRequestPoint)
        {
            this.numRequestPoint = numRequestPoint;
            points.Clear();
            point3Ds.Clear();
            actionEveryPoint = null;
            actionEveryLastTwoPoints = null;
            actionPointInputEnd = null;
            viewType = DRAW.SelectionWindow.ViewType.Line;

            //중복실행되는 경우 초기화
            main.MouseLeave -= RequestUserCoordinates_End;
            main.MouseDown -= PutUserClickInput_MouseLeftDown;
            main.KeyDown -= RequestUserCoordinates_EscKey;
            MouseMoveOn(false);
        }
        internal void Start()
        {
            on = true;
            main.WindowSelectionOn(false);
            main.TurnOnDeselectAll_Esc(false);
            main.draw.selectionWindow.viewType = viewType;

            //main.MouseLeave += RequestUserCoordinates_End;
            main.KeyDown += RequestUserCoordinates_EscKey;
            main.MouseDown += PutUserClickInput_MouseLeftDown;

            main.cmd.SendRequestMessage("Specify first point");

            //if (numPoint==1)
            //{
            //    // 사용자 입력 윈도우의 첫번째 포인트가 이미 입력된 경우.
            //    //if (main.orbiting) return;
            //    main.draw.selectionWindow.Start(points[0]);
            //    main.MouseMove += RequestUserCoordinates_MouseMove;
            //    main.MouseLeave += RequestUserCoordinates_End;
            //    main.KeyDown += RequestUserCoordinates_EscKey;
            //}
            //else
            //{
            //    main.MouseDown += RequestUserCoordinates_MouseLeftDown;
            //}
        }
        private bool mouseMoveOn = false;
        private void MouseMoveOn(bool on)
        {
            if (on)
            {
                if (!mouseMoveOn) //이벤트 중복 생성 방지
                {
                    main.MouseMove += RequestUserCoordinates_MouseMove;
                    MoveMouseLittle(); //마우스 이벤트 걸자마자 한번 움직이게~ 이걸 안하면 직선이 엉뚱한데 날라간 상태로 시작됨.
                }
                main.draw.selectionWindow.Start(points[points.Count - 1]);
            }
            else
            {
                main.MouseMove -= RequestUserCoordinates_MouseMove;
                main.draw.selectionWindow.End();
            }
            mouseMoveOn = on;
        }
        private void MoveMouseLittle()
        {
            //마우스 커서를 살짝 움직임. 마우스 이벤트 강제 발생용.
            System.Drawing.Point p = System.Windows.Forms.Cursor.Position;
            p.X += 1;
            System.Windows.Forms.Cursor.Position = p;

            //살짝 움직였다가 다시 돌아오는 경우 마우스 이벤트 발생 안함. 그냥 움직인 상태로 나두는게 좋을 듯.
            //p.X -= 1;
            //System.Windows.Forms.Cursor.Position = p;
        }
        private void CallFunctionAfterPointInput(Point p0)
        {
            actionEveryPoint?.Invoke(p0); //if(actionEveryPoint!=null) actionEveryPoint(p0);
            if (points.Count > 1)
            {
                actionEveryLastTwoPoints?.Invoke(points[points.Count - 2], points[points.Count - 1]);
            }

            if (numRequestPoint == points.Count)
            {
                End();

                actionPointInputEnd?.Invoke(points);
                actionEnd?.Invoke();
                //p0 = points[points.Count-1];

                //main.bckD.GetInfinitePyramidBySelectionWindow(selectWindowStart, selectWindowEnd, ref p0, ref v0, ref v1, ref v2, ref v3);
                //main.fem.SelectByInfinitePyramid(p0, v0, v1, v2, v3);
                main.RedrawFemModel();
                return;
            }
            main.RedrawFemModel();
            MouseMoveOn(true);
            main.cmd.SendRequestMessage("Specify next point");
        }
        private void RequestUserCoordinates_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (main.draw.selectionWindow.started)
            {
                Point p0 = e.GetPosition(main.grdMain);
                main.draw.selectionWindow.Move(p0);
            }
            //bckD.DrawSelectionWindow(selectWindowStart, selectWindowEnd);
        }
        private void RequestUserCoordinates_EscKey(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                End();
            }
        }
        private void RequestUserCoordinates_End(object sender, System.Windows.Input.MouseEventArgs e)
        {
            End();
        }
        public void End()
        {
            on = false;
            main.MouseLeave -= RequestUserCoordinates_End;
            main.MouseDown -= PutUserClickInput_MouseLeftDown;
            main.KeyDown -= RequestUserCoordinates_EscKey;
            MouseMoveOn(false);
            //main.cmd.Enter();
            main.cmd.NewLine();

            main.WindowSelectionOn(true);
            main.TurnOnDeselectAll_Esc(true);
        }

        private void PutUserClickInput_MouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            //if (main.orbiting) return;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                main.cmd.Enter();
                Point p0 = e.GetPosition(main.grdMain);
                Put(p0);
            }
        }
        private void AddPoint(Point p0)
        {
            points.Add(p0);
            Point3D p3d = GetPoint3dFromPoint2D(p0);
            point3Ds.Add(p3d);
        }
        private Point AddPoint(Point3D p3d)
        {
            point3Ds.Add(p3d);
            Point p0 = GetPointFromPoint3D(p3d);
            points.Add(p0);
            return p0;
        }

        internal void Put(Point3D userInputPoint3D)
        {
            Point p0 = AddPoint(userInputPoint3D);
            CallFunctionAfterPointInput(p0);
        }
        internal void Put(Point userInputPoint)
        {
            AddPoint(userInputPoint);
            CallFunctionAfterPointInput(userInputPoint);
        }
        private Point3D GetPoint3dFromPoint2D(Point p0)
        {
            return main.draw.GetPoint3dOnBasePlane_FromPoint2D(p0);
        }
        private Point GetPointFromPoint3D(Point3D p3d)
        {
            return main.draw.GetPoint2D_FromPoint3D(p3d);
        }

        Point beforPanMove;
        internal void PanMoveStart()
        {
            if (points.Count > 0)
            {
                beforPanMove = points[points.Count - 1];
            }
        }
        internal void PanMove(Vector mov)
        {
            points[points.Count - 1] = beforPanMove + mov;
            main.draw.selectionWindow.wP0 = beforPanMove + mov;
        }
        internal void PanMoveEnd()
        {

        }
    } //사용자 입력 요청
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

    public class CommandWindow
    {
        private readonly MainWindow main;
        private readonly System.Windows.Controls.TextBox textBox;

        /// <summary>
        /// 모든 command의 상위 command, root command.
        /// </summary>
        private readonly Command rC;
        private Command activeCommand;
        private Command lastCommand;

        private string userInput;
        private Point3D userInputPoint3D;
        private Vector3D userInputVector3D;
        private double userInputDouble;
        private int userInputInt;

        private readonly string initialCmdMark = "Command";
        private readonly string cmdMark = ": ";

        public CommandWindow(MainWindow mainWindow, System.Windows.Controls.TextBox textBox)
        {
            main = mainWindow;
            Command.main = mainWindow;
            this.textBox = textBox;

            rC = new Command("Main");
            activeCommand = rC;

            Clear();
            SetCommandStructure();

            textBox.PreviewKeyDown += Tbx_PreviewKeyDown; //space, enter, backspace 처리
            textBox.KeyDown += Tbx_KeyDown; //esc 처리
            textBox.KeyUp += Tbx_KeyUp; //space, enter 후처리
        }

        private void SetCommandStructure()
        {
            Command cmd, subCmd;
            cmd = rC.Add("Regen", "re"); cmd.run += main.RedrawFemModel;
            cmd = rC.Add("Redraw", "r"); cmd.run += main.RedrawShapes;

            cmd = rC.Add("Zoom", "z");
            subCmd = cmd.Add("All", "a"); subCmd.run += main.ZoomAll;
            subCmd = cmd.Add("Extents", "e"); subCmd.run += main.ZoomExtents;
            subCmd = cmd.Add("Window", "w"); subCmd.run += main.ZoomWindow;

            cmd = rC.Add("Circle", "ci"); subCmd.run += main.DrawCicleCenterRadius;
            subCmd = cmd.Add("Radius", "r"); subCmd.run += main.DrawCicle;

            cmd = rC.Add("Erase", "e"); cmd.runSelected += main.EraseSelected;
            subCmd = cmd.Add("All", "a"); subCmd.run += main.EraseAll;
            subCmd = cmd.Add("Fence", "f"); subCmd.run += main.EraseFence;

            cmd = rC.Add("Line", "l"); cmd.run += main.AddLine;

            cmd = rC.Add("Divide", "d"); cmd.run += main.DivideElem;

            cmd = rC.Add("Extrude", "ex"); cmd.run += main.ExtrudeElem;

        } //명령어 구성!!!

        private void GetCommand()
        {
            //입력 명령어 추출
            userInput = "";
            for (int i = textBox.Text.Length - cmdMark.Length; i >= 0; i--)
            {
                string a = textBox.Text.Substring(i, cmdMark.Length);
                if (textBox.Text.Substring(i, cmdMark.Length).Equals(cmdMark))
                {
                    int cmdStrPoint = i + cmdMark.Length;
                    userInput = textBox.Text.Substring(cmdStrPoint, textBox.Text.Length - cmdStrPoint).Trim();
                    break;
                }
            }

            //명령어 없이 스페이스만 누르는 경우
            if (userInput.Equals(""))
            {
                if (lastCommand == null)
                {
                    //WriteText("직전에 실행한 명령이 없습니다.");
                    Enter();
                    NewLine();
                    return;
                }
                if (main.requestUserCoordinatesInput.on) //이미 명령이 실행중인 경우 종료
                {
                    main.requestUserCoordinatesInput.End();
                }
                if (main.requestUserInput.On) //이미 명령이 실행중인 경우 다시 실행
                {
                    Enter();
                    main.requestUserInput.DoAction();
                }
                else
                {
                    WriteText("(last command) " + lastCommand.name);
                    Enter();
                    ExecuteCommand(lastCommand); // 직전 명령 다시 실행
                }
                return;
            }

            //대문자로 변경
            userInput = userInput.ToUpper();

            //입력 명령어와 동일한 명령 실행
            foreach (Command cmd in activeCommand.commands)
            {
                if (userInput.Equals(cmd.shortName.ToUpper()))
                {
                    WriteText(" " + cmd.name);
                    Enter();
                    ExecuteCommand(cmd);
                    return;
                }
                if (userInput.Equals(cmd.name.ToUpper()))
                {
                    Enter();
                    ExecuteCommand(cmd);
                    return;
                }
            }

            //입력값이 상대좌표인 경우. @로 시작하는 경우 상대좌표로 인식.
            if (userInput.Substring(0, 1).Equals("@"))
            {
                int isRelativeCoordinateInput = IsRelativeCoordinateInput(userInput.Substring(1));
                if (isRelativeCoordinateInput >= 0) Enter();
                switch (isRelativeCoordinateInput)
                {
                    case 2:
                        if (main.requestUserInput.On)
                        {
                            main.requestUserInput.Put(userInputVector3D);
                        }
                        return;
                    case 3:
                        if (main.requestUserInput.On)
                        {
                            main.requestUserInput.Put(userInputVector3D);
                        }
                        return;
                    default:
                        break;
                }
            }

            //입력값이 좌표인 경우
            int isCoordinateInput = IsCoordinateInput(userInput);
            if (isCoordinateInput >= 0)
            {
                Enter();

                //상대좌표를 요청했는데 절대좌표가 입력된 경우.
                if (main.requestUserInput.doingActionType == UserInputAction.RequestInputType.Direction)
                {
                    main.requestUserInput.PutDirectionFirstPoint(userInputPoint3D);

                    return;
                }

                switch (isCoordinateInput)
                {
                    case 0:
                        if (main.requestUserInput.On)
                        {
                            main.requestUserInput.Put(userInputInt);
                        }
                        return;
                    case 1:
                        if (main.requestUserInput.On)
                        {
                            main.requestUserInput.Put(userInputDouble);
                        }
                        return;
                    case 2:
                        if (main.requestUserCoordinatesInput.on)
                        {
                            main.requestUserCoordinatesInput.Put(userInputPoint3D);
                        }
                        return;
                    case 3:
                        if (main.requestUserCoordinatesInput.on)
                        {
                            main.requestUserCoordinatesInput.Put(userInputPoint3D);
                        }
                        return;
                    default:
                        break;
                }

            }


            //상대좌표를 요청했는데 입력되지 않은 경우.
            if (main.requestUserInput.doingActionType == UserInputAction.RequestInputType.Direction)
            {
                WriteText(" (상대좌표값이 아닙니다. 상대좌표를 입력하세요. ex: @0,1 or @0,1,0)");
                Enter();
                main.requestUserInput.DoAction();
                return;
            }

            //명령어가 없는 경우.
            WriteText(" Unknown command.");
            Enter();
            if (activeCommand == rC)
            {
                textBox.AppendText(initialCmdMark + cmdMark);
            }
            else
            {
                WriteText(activeCommand.GetSubCmdQuaryString());
                textBox.AppendText(cmdMark);
            }
            SetCursorLast();
            return;
        }
        private void ExecuteCommand(Command cmd)
        {
            lastCommand = cmd;

            if (main.orbiting) main.TurnOnOrbit(false);

            if (main.fem.selection.Count > 0 & cmd.runSelected != null) //선택된 개체가 있고, cmd.runSelected를 지정한 경우.
            {
                WriteText("선택된 개체의 " + cmd.name + "을(를) 실행합니다.");
                Enter();
                cmd.runSelected();
                AfterCommandRun();
                return;
            }
            if (cmd.run != null) //cmd.run이 지정된 경우
            {
                cmd.run();
                AfterCommandRun();
                return;
            }
            if (cmd.commands.Count == 0) //서브명령 개수가 0인 경우
            {
                SendRequestMessage("개체를 선택하고 실행하세요.");
                return;
            }

            //서브명령 선택 요청
            WriteText(cmd.GetSubCmdQuaryString());
            activeCommand = cmd;
            WriteText(cmdMark);
        }
        private void AfterCommandRun()
        {
            if(main.requestUserInput == null)
            {
                SetForOtherCommand();
                return;
            }
            if (main.requestUserCoordinatesInput.on | main.requestUserInput.On)
            {
            }
            else
            {
                SetForOtherCommand();
            }

            return;
            void SetForOtherCommand()
            {
                activeCommand = rC;
                NewLine();
            }
        }

        /// <summary>
        ///  userInput의 입력값 종류 판별값 반환.
        ///  0: int
        ///  1: double
        ///  2: 2차원 좌표.
        ///  3: 3차원 좌표.
        ///  -1: 아무것도 아닌 경우.
        ///  좌표인 경우 userInputPoint3D에 Point3D값을 저장
        /// </summary>
        /// <param name="userInput"></param>
        /// <returns>리턴즈ㅋㅋ</returns>
        private int IsCoordinateInput(string userInput)
        {
            int falseResult = -1;
            //인풋 텍스트에 쉼표가 1-2개 있고 그 사이값이 전부 double인 경우 true 반환
            int firstCommaLocation = userInput.IndexOf(",");
            double doubleValue;
            int intValue;
            if (firstCommaLocation == -1)
            {
                try
                {
                    doubleValue = double.Parse(userInput);
                }
                catch (Exception)
                {
                    return falseResult;
                }
                intValue = (int)doubleValue;
                if (doubleValue.Equals((double)intValue))
                {
                    userInputInt = intValue;
                    return 0;
                }
                else
                {
                    userInputDouble = doubleValue;
                    return 1;
                }
            }
            string firstString = userInput.Substring(0, firstCommaLocation);
            double firstValue;
            if (firstCommaLocation + 1 > userInput.Length)
            {
                return falseResult;
            }
            try
            {
                firstValue = double.Parse(firstString);
            }
            catch (Exception)
            {
                return falseResult;
            }

            string restString = userInput.Substring(firstCommaLocation + 1);

            int secondCommaLocation = restString.IndexOf(",");
            string secondString;
            double secondValue = 0;
            if (secondCommaLocation < 0)
            {
                secondString = restString;
                try
                {
                    secondValue = double.Parse(secondString);
                    userInputPoint3D = new Point3D(firstValue, secondValue, 0);
                    return 2;
                }
                catch (Exception)
                {
                    return falseResult;
                }
            }

            secondString = restString.Substring(0,secondCommaLocation);
            try
            {
                secondValue = double.Parse(secondString);
            }
            catch (Exception)
            {
                return falseResult;
            }

            restString = restString.Substring(secondCommaLocation + 1);
            string thirdString = restString.Substring(0, restString.Length);
            double thirdValue;
            try
            {
                thirdValue = double.Parse(thirdString);
                userInputPoint3D = new Point3D(firstValue, secondValue, thirdValue);
                return 3;
            }
            catch (Exception)
            {
                return falseResult;
            }

        } //사용자 입력에 의한 userInputPoint3D 반환
        private int IsRelativeCoordinateInput(string userInput)
        {
            int isCoordinateInput = IsCoordinateInput(userInput);
            userInputVector3D = new Vector3D(userInputPoint3D.X, userInputPoint3D.Y, userInputPoint3D.Z);
            return isCoordinateInput;
        } //사용자 입력에 의한 userInputPoint3D 반환

        internal class Command
        {
            internal static MainWindow main;
            internal Run run; //하위 Command가 있든 없든 무조건 실행
            internal delegate void Run();
            internal Run runSelected; //이미 선택된 개체가 있는 경우 실행
            internal RunMouse runMouseDown; //하위명령 커리 중 마우스를 클릭하는 경우 실행
            internal delegate void RunMouse(Point p0);
            internal string name; //ex. _zoom , _line
            internal string shortName = "";

            public List<Command> commands = new List<Command>();
            private Command mouseDownSubCommand;

            public Command(string name, string shortName = "")
            {
                this.name = name;
                this.shortName = shortName;
            }
            public Command Add(string name, string shortName = "")
            {
                Command subCmd = new Command(name, shortName);
                commands.Add(subCmd);
                return subCmd;
            } // 서브 명령 추가.

            internal string GetSubCmdQuaryString()
            {
                //cmd가 subCmd를 가지고 있는 경우 사용자에게 subCmd 선택을 요청하는 메시지를 생성함.
                //string quary = " : ";
                string quary = "";
                foreach (Command cmd in commands)
                {
                    quary += cmd.name;
                    if (cmd.runMouseDown != null)
                    {
                        //main.MouseDown += Main_MouseDown;
                        mouseDownSubCommand = cmd;
                        quary += " (window)";
                    }
                    quary += " / ";
                }
                quary = quary.Substring(0, quary.Length - 2);
                return quary;
            }

            private void Main_MouseDown(object sender, MouseButtonEventArgs e)
            {
                main.MouseDown -= Main_MouseDown;
                if(e.LeftButton == MouseButtonState.Pressed)
                {
                    Point p0 = e.GetPosition(main.grdMain);
                    mouseDownSubCommand.runMouseDown(p0);
                }
            }
        }
        private void Tbx_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    GetCommand();
                    break;
                case Key.Enter:
                    GetCommand();
                    //if (userInput.Equals("")) WriteText("> ");
                    //WriteText(userInput);
                    break;
                case Key.Back:
                    String a = textBox.Text.Substring(textBox.Text.Length - 2, cmdMark.Length);
                    if (textBox.Text.Substring(textBox.Text.Length - 2, cmdMark.Length).Equals(cmdMark))
                    {
                        Space();
                    }
                    break;
            }
        }
        private void Tbx_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    if (activeCommand != rC)
                    {
                        Cancel();
                    }
                    else
                    {
                        Cancel();
                    }
                    break;
            }
        }
        private void Tbx_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    //BackSpace();
                    break;
                case Key.Enter:
                    //WriteText("> ");
                    BackSpace(2); //그냥 enter만 누른 경우 한줄 위로 올림
                    break;
                default:
                    break;
            }
        }

        private void Cancel()
        {
            WriteText("*Cancel*");
            Enter();
            NewLine();
            activeCommand = rC;
        }
        private void Clear()
        {
            textBox.Focus();
            textBox.Text = initialCmdMark + cmdMark;
            SetCursorLast();
        }
        private void Space()
        {
            textBox.AppendText(" ");
            SetCursorLast();
        }


        public void NewLine()
        {
            textBox.Focus();
            textBox.AppendText(initialCmdMark + cmdMark);
            SetCursorLast();
        }
        private void BackSpace(int length = 1)
        {
            textBox.Text = textBox.Text.Substring(0, textBox.Text.Length - length);
            SetCursorLast();
        }
        public void Enter()
        {
            textBox.AppendText(Environment.NewLine);
            SetCursorLast();
        }
        private void WriteText(String text)
        {
            textBox.AppendText(text);
            SetCursorLast();
        }
        private void SetCursorLast()
        {
            textBox.Select(textBox.Text.Length, 0);
        }
        public void SendRequestMessage(string message)
        {
            WriteText(message);
            WriteText(cmdMark);
        }

        internal void CallCommand(string v)
        {
            WriteText(v);
            SetCursorLast();
            GetCommand();
        }

        internal void ErrorMessage(string v)
        {
            WriteText("Error!!! " + v);
            NewLine();
        }
    } //명령창 명령어 관리

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
                                draw.shapes.AddBox(p.nodes[0].c0, p.nodes[2].c0 - p.nodes[0].c0);
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
            requestUserCoordinatesInput.Reset(2);
            requestUserCoordinatesInput.viewType = DRAW.SelectionWindow.ViewType.Rectangle;
            requestUserCoordinatesInput.actionEveryLastTwoPoints += draw.ViewZoomRectangle;
            requestUserCoordinatesInput.Start();
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
                if (requestUserCoordinatesInput.on)
                {
                    requestUserCoordinatesInput.PanMoveStart();
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
                if (requestUserCoordinatesInput.on)
                {
                    requestUserCoordinatesInput.PanMove(mov);
                }
                GetCameraInfo();
            }
        }
        private void PanOff_MouseWheelUp(object sender, MouseButtonEventArgs e)
        {
            draw.OrbitEnd();
            if (requestUserCoordinatesInput.on)
            {
                requestUserCoordinatesInput.PanMoveEnd();
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