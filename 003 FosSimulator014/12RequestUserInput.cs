using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace _003_FosSimulator014
{
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
            actionEnd?.Invoke();
            End_Cancle();
        }
        internal void End_Cancle()
        {
            On = false;
            ClearActions();
            TurnOffAllEvents();
            
        }
        private void TurnOffAllEvents()
        {
            main.KeyDown -= ExitCommand_EscKey;
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
        internal delegate void inputInts(List<int> n);
        internal inputInts actionAfterIntsWithInts;
        internal delegate void inputDirInt(Vector3D dir, int n);
        internal inputDirInt actionAfterIntWithDirInt;
        internal delegate void inputP3dP3d(Point3D p0, Point3D p1);
        internal inputP3dP3d actionEveryLastTwoPointsWithPointPoint;
        internal delegate void inputP2dP2d(Point p0, Point p1);
        /// <summary>
        /// 사용자가 Command창에서 스페이스마로 Points의 입력을 끝냈을 때 실행할 Action을 지정합니다.
        /// List<Point>를 넘겨줍니다.
        /// </summary>
        internal inputPoints actionAfterPoints;
        internal delegate void inputPoints(List<Point3D> points);
        /// <summary>
        /// 사용자 입력이 끝났을 때 실행할 Action을 지정합니다.
        /// 넘겨주는 값이 없습니다.
        /// 입력값이 없는 함수를 지정해야 합니다.
        /// </summary>
        internal Action actionEnd;
        internal Action actionEveryLastTwoPoints;
        private void ClearActions()
        {
            actionAfterIntWithInt = null;
            actionAfterIntWithDirInt = null;
            actionEveryLastTwoPointsWithPointPoint = null;
            actionEnd = null;
        }

        private UserInputAction LastAction
        {
            get
            {
                return userInputActions[userInputActions.Count - 1];
            }
        }
        private readonly List<UserInputAction> userInputActions = new List<UserInputAction>();
        private int actionStep = 0;
        internal DRAW.SelectionWindow.ViewType viewType;

        private Vector3D userInputVector;
        private double userInputDouble;
        private Point3D userInputPoint;
        private List<Point3D> userInputPoints;

        internal void RequestInt(string message)
        {
            UserInputAction userInputAction = new UserInputAction
            {
                requestInputType = UserInputAction.RequestInputType.Int,
                message = message
            };
            userInputActions.Add(userInputAction);
        }
        internal void RequestInts(string message)
        {
            UserInputAction userInputAction = new UserInputAction
            {
                requestInputType = UserInputAction.RequestInputType.Ints,
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
        internal void RequestPoints(string message)
        {
            UserInputAction userInputAction = new UserInputAction
            {
                requestInputType = UserInputAction.RequestInputType.Points,
                message = message
            };
            userInputActions.Add(userInputAction);
        }
        internal void RequestPoints(int numPoint)
        {
            UserInputAction userInputAction = new UserInputAction
            {
                requestInputType = UserInputAction.RequestInputType.Points,
                message = R.String8,
                numPointRequested = numPoint,
                viewType = viewType
            };
            userInputActions.Add(userInputAction);
        }

        internal void Start()
        {
            actionStep = -1;
            On = true;
            NextAction();
        }
        private void NextAction()
        {
            actionStep += 1;
            DoAction();
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
                    main.cmd.viewType = viewType;
                    main.cmd.RequestInput_Points(userInputAction.message, userInputAction.numPointRequested);
                    numContinuousPoint = 0;
                    main.cmd.actionAfterPoint += Put_ContinuousPoint;
                    main.cmd.actionAfterPoints += Put;
                    return;
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
                    main.cmd.RequestInput_Int(userInputAction.message);
                    main.cmd.actionAfterIntWithInt += Put;
                    return;
                case UserInputAction.RequestInputType.Ints:
                    main.cmd.RequestInput_Ints(userInputAction.message);
                    main.cmd.actionAfterIntsWithInts += Put;
                    return;
                case UserInputAction.RequestInputType.Double:
                    break;
                case UserInputAction.RequestInputType.Distance:
                    break;
                case UserInputAction.RequestInputType.Direction:
                    //main.cmd.SendRequestMessage(userInputAction.message);
                    //main.MouseDown += GetDirection;
                    main.cmd.RequestInput_Direction(userInputAction.message);
                    main.cmd.actionAfterDirWithDir += Put;
                    return;
                default:
                    break;
            }
            main.cmd.ErrorMessage(R.String9);
            End();
            return;
        } //Command에 요청하거나 액션 수행.

        internal void Put(int userInputInt)
        {
            actionAfterIntWithInt?.Invoke(userInputInt);
            actionAfterIntWithDirInt?.Invoke(userInputVector, userInputInt);
            NextAction();
        }
        internal void Put(List<int> userInputInts)
        {
            actionAfterIntsWithInts?.Invoke(userInputInts);
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
        internal void Put(Point3D userInputPoint)
        {
            this.userInputPoint = userInputPoint;
            NextAction();
        }
        private int numContinuousPoint;

        internal void Put_ContinuousPoint(Point3D userInputPoint)
        {
            if (numContinuousPoint != 0)
            {
                actionEveryLastTwoPointsWithPointPoint?.Invoke(this.userInputPoint, userInputPoint);
                actionEveryLastTwoPoints?.Invoke();
            }

            this.userInputPoint = userInputPoint;
            numContinuousPoint += 1;
        }
        internal void Put(List<Point3D> userInputPoints)
        {
            this.userInputPoints = userInputPoints;
            NextAction();
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
    public class RequestUserCoordinatesInput_Old
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
        internal ActionPointList actionPointInputEnd;
        internal delegate void ActionPointList(List<Point> Plist);
        internal ActionWithNone actionEnd;
        internal delegate void ActionWithNone();

        public RequestUserCoordinatesInput_Old(MainWindow main)
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
            if (points.Count >= 1)
            {
                points[points.Count - 1] = beforPanMove + mov;
                main.draw.selectionWindow.wP0 = beforPanMove + mov;
            }
        }
        internal void PanMoveEnd()
        {

        }
    } //사용자 입력 요청

}