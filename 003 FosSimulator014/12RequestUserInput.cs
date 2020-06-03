using BCK.SmrSimulation.Draw2D;
using BCK.SmrSimulation.Draw3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace BCK.SmrSimulation.Main
{
    /// <summary>
    /// 사용자가 요청한 하나하나의 실행 단위.
    /// </summary>
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
            /// <summary>
            /// Vector3D 형식의 입력값. Vector로 요청했는데 사용자가 Point를 입력(마우스 클릭, 커멘트 창에 절대좌표로 입력)한 경우, 0,0,0을 기준으로한 벡터로 볼 수 있을지 검토 필요.
            /// </summary>
            Vector,
            VectorValue,
        }
        internal RequestInputType requestInputType;
        internal string message;
        internal int numPointRequested = -1;
        internal MouseInputGuide.ViewType viewType;
    }

    /// <summary>
    /// 사용자 입력 요청 관리 클래스. command 창과 연동하여 작동함.
    /// </summary>
    public class RequestUserInput
    {
        private MainWindow main;
        public RequestUserInput(MainWindow main)
        {
            this.main = main;
        }

        private bool isOn = false;
        internal bool IsOn
        {
            get
            {
                return isOn;
            }
            set
            {
                if (value)
                {
                    if (isOn) return;
                    main.IsOnWindowSelect = false;
                    main.IsOnDeselectAllByEsc = false;
                    main.KeyDown += ExitCommand_EscKey;
                }
                else
                {
                    main.IsOnWindowSelect = true;
                    main.IsOnDeselectAllByEsc = true;
                    main.KeyDown -= ExitCommand_EscKey;
                }
                isOn = value;
            }
        }
        internal void End()
        {
            actionEnd?.Invoke();
            IsOn = false;
        }
        private void ExitCommand_EscKey(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                ClearActions();
                IsOn = false;
            }
        }

        /// <summary>
        /// 본 RequestUserInput에서 처리하도록 지정된 Action에서 사용자에게 입력을 요구하는 InputType입니다. (int, real, 좌표 등)
        /// </summary>
        private UserInputAction.RequestInputType doingActionInputType;

        /// <summary>
        /// 지정된 모든 action들을 null로 초기화합니다. 이전 명령에서 지정한 액션들이 모두 초기화 됩니다.
        /// </summary>
        internal void ClearActions()
        {
            actionAfterIntWithInt = null;
            actionAfterIntsWithInts = null;
            actionAfterIntWithVecInt = null;
            actionAfterVecWithVec = null;
            actionAfterEveryLastPointWithPoint = null;
            actionEveryLastTwoPointsWithPointPoint = null;
            actionAfterPoints = null;
            actionEnd = null;
            actionEveryLastTwoPoints = null;
            actionAfterEveryPoint = null;
        }  // delegate를 추가할 때 꼭 여기도 추가할 것.


        internal delegate void inputInt(int n);
        internal inputInt actionAfterIntWithInt;
        internal delegate void inputInts(List<int> n);
        internal inputInts actionAfterIntsWithInts;
        internal delegate void inputVecInt(Vector3D vec, int n);
        internal inputVecInt actionAfterIntWithVecInt;
        internal delegate void inputVec(Vector3D vec);
        internal inputVec actionAfterVecWithVec;

        internal delegate void inputP3d(Point3D p0);
        /// <summary>
        /// 사용자가 Point를 입력할 때 마다 실행한 Action을 지정합니다.
        /// Point3D를 넘겨줍니다.
        /// </summary>
        internal inputP3d actionAfterEveryLastPointWithPoint;
        internal delegate void inputP3dP3d(Point3D p0, Point3D p1);
        internal inputP3dP3d actionEveryLastTwoPointsWithPointPoint;
        internal delegate void inputP2dP2d(Point p0, Point p1);

        /// <summary>
        /// 사용자가 Command창에서 스페이스바로 Points의 입력을 끝냈을 때 실행할 Action을 지정합니다.
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
        internal Action actionAfterEveryPoint;

        private UserInputAction LastAction
        {
            get
            {
                return userInputTypes[userInputTypes.Count - 1];
            }
        }
        private readonly List<UserInputAction> userInputTypes = new List<UserInputAction>();
        private int actionStep = 0;
        internal MouseInputGuide.ViewType viewType;

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
            userInputTypes.Add(userInputAction);
        }
        internal void RequestInts(string message)
        {
            UserInputAction userInputAction = new UserInputAction
            {
                requestInputType = UserInputAction.RequestInputType.Ints,
                message = message
            };
            userInputTypes.Add(userInputAction);
        }
        /// <summary>
        /// 사용자에게 벡터를 입력하도록 요청. 사용자가 절대좌표를 입력하는 경우 두번째 절점을 입력하도록 다시 요청하여 벡터 반환.
        /// </summary>
        internal void RequestVector(string message)
        {
            UserInputAction userInputAction = new UserInputAction
            {
                requestInputType = UserInputAction.RequestInputType.Vector,
                message = message
            };
            userInputTypes.Add(userInputAction);
        }
        /// <summary>
        /// 사용자에게 벡터를 입력하도록 요청. 사용자가 키보드로 절대좌표를 입력한 경우는 입력값을 벡터로 반환. 마우스로 첫번째 점을 입력한 경우는 두번째 점을 요청하여 벡터 반환.
        /// </summary>
        internal void RequestVectorValue(string message)
        {
            UserInputAction userInputAction = new UserInputAction
            {
                requestInputType = UserInputAction.RequestInputType.VectorValue,
                message = message
            };
            userInputTypes.Add(userInputAction);
        }
        internal void RequestPoints(string message)
        {
            UserInputAction userInputAction = new UserInputAction
            {
                requestInputType = UserInputAction.RequestInputType.Points,
                message = message
            };
            userInputTypes.Add(userInputAction);
        }
        internal void RequestPoints(int numPoint)
        {
            UserInputAction userInputAction = new UserInputAction
            {
                requestInputType = UserInputAction.RequestInputType.Points,
                message = Properties.Resource.String8,
                numPointRequested = numPoint,
                viewType = viewType
            };
            userInputTypes.Add(userInputAction);
        }
        internal void RequestNodeSelection(string message)
        {
            UserInputAction userInputAction = new UserInputAction
            {
                requestInputType = UserInputAction.RequestInputType.NodeSelection,
                message = message
            };
            userInputTypes.Add(userInputAction);
        }
        internal void RequestElemSelection(string message)
        {
            UserInputAction userInputAction = new UserInputAction
            {
                requestInputType = UserInputAction.RequestInputType.ElemSelection,
                message = message
            };
            userInputTypes.Add(userInputAction);
        }

        /// <summary>
        /// 구성된 RequestUserInput을 실행합니다.
        /// </summary>
        internal void Start()
        {
            actionStep = -1;
            IsOn = true;
            NextAction();
        }
        private void NextAction()
        {
            actionStep += 1;
            DoAction();
        }
        internal void DoAction()
        {
            if (actionStep >= userInputTypes.Count)
            {
                End();
                return;
            }
            UserInputAction userInputType = userInputTypes[actionStep];
            doingActionInputType = userInputType.requestInputType;
            switch (userInputType.requestInputType)
            {
                case UserInputAction.RequestInputType.Point:
                    break;
                case UserInputAction.RequestInputType.TwoPoints:
                    break;
                case UserInputAction.RequestInputType.Points:
                    main.Cmd.viewType = viewType;
                    main.Cmd.RequestInput_Points(userInputType.message, userInputType.numPointRequested);
                    numContinuousPoint = 0;
                    main.Cmd.actionAfterPointWithPoint += Put_ContinuousPoint;
                    main.Cmd.actionAfterPoints += Put;
                    return;
                case UserInputAction.RequestInputType.ElemSelection:
                    if (main.Fem.Selection.elems.Count == 0)
                    {
                        main.Cmd.SendRequestMessage(userInputType.message);
                        End();
                    }
                    else
                    {
                        NextAction();
                    }
                    return;
                case UserInputAction.RequestInputType.NodeSelection:
                    if (main.Fem.Selection.nodes.Count == 0)
                    {
                        main.Cmd.SendRequestMessage(userInputType.message);
                        End();
                    }
                    else
                    {
                        NextAction();
                    }
                    return;
                case UserInputAction.RequestInputType.Selection:
                    if (main.Fem.Selection.nodes.Count + main.Fem.Selection.elems.Count == 0)
                    {
                        main.Cmd.SendRequestMessage(userInputType.message);
                        End();
                    }
                    else
                    {
                        NextAction();
                    }
                    return;
                case UserInputAction.RequestInputType.Int:
                    main.Cmd.RequestInput_Int(userInputType.message);
                    main.Cmd.actionAfterIntWithInt += Put;
                    return;
                case UserInputAction.RequestInputType.Ints:
                    main.Cmd.RequestInput_Ints(userInputType.message);
                    main.Cmd.actionAfterIntsWithInts += Put;
                    return;
                case UserInputAction.RequestInputType.Double:
                    break;
                case UserInputAction.RequestInputType.Distance:
                    break;
                case UserInputAction.RequestInputType.Vector:
                    //main.cmd.SendRequestMessage(userInputAction.message);
                    //main.MouseDown += GetDirection;
                    main.Cmd.RequestInputVector(userInputType.message);
                    main.Cmd.actionAfterVecWithVec += Put;
                    return;
                case UserInputAction.RequestInputType.VectorValue:
                    main.Cmd.RequestInputVectorValue(userInputType.message);
                    main.Cmd.actionAfterVecWithVec += Put;
                    return;
                default:
                    break;
            }
            main.Cmd.ErrorMessage(Properties.Resource.String9);
            End();
            return;
        } //Command에 요청하거나 액션 수행.

        internal void Put(int userInputInt)
        {
            actionAfterIntWithInt?.Invoke(userInputInt);
            actionAfterIntWithVecInt?.Invoke(userInputVector, userInputInt);
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
            actionAfterVecWithVec?.Invoke(userInputVector);
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

            actionAfterEveryLastPointWithPoint?.Invoke(userInputPoint);
            actionAfterEveryPoint?.Invoke();

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
            return main.Draw.GetPoint3dOnBasePlaneFromPoint2D(p0);
        }
        private Point GetPointFromPoint3D(Point3D p3d)
        {
            return main.Draw.GetPoint2DFromPoint3D(p3d);
        }

    }

    /// <summary>
    /// 개체선택 전용 사용자 입력 요청 클래스.
    /// RequestUserInput 클래스와 동일하지만 개체를 선택하는 경우 Command창 입력이 필요 없는 등 다른 인풋 요청과 다른 특성들이 있어서 별도 클래스로 실행함.
    /// </summary>
    public class RequestUserMouseWindowInput
    {
        private readonly MainWindow main;
        private Point p0, p1;

        internal delegate void Action(Point p0, Point p1);
        internal Action action;
        private bool hasFirstPoint = false;
        private Point firstPoint;
        internal MouseInputGuide.ViewType viewType;

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
            main.IsOnWindowSelect = false;
            main.mouseInputGuide.viewType = viewType;

            if (hasFirstPoint)
            {
                // 사용자 입력 윈도우의 첫번째 포인트가 이미 입력된 경우.
                if (main.IsOnOrbit) return;
                p0 = FirstPoint;
                main.mouseInputGuide.Start(p0);
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
            if (main.IsOnOrbit) return;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                p0 = e.GetPosition(main.grdMain);
                main.mouseInputGuide.Start(p0);
                main.MouseMove += WindowSelection_MouseMove;
                main.MouseUp += WindowSelection_MouseLeftUp;
                main.MouseLeave += WindowSelectionEnd;
            }
        }
        private void WindowSelection_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            p1 = e.GetPosition(main.grdMain);
            main.mouseInputGuide.Move(p1);
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
            main.mouseInputGuide.End();
            main.MouseMove -= WindowSelection_MouseMove;
            main.MouseUp -= WindowSelection_MouseLeftUp;
            main.MouseLeave -= WindowSelectionEnd;
            main.MouseDown -= WindowSelection_MouseLeftDown;

            main.IsOnWindowSelect = true;
        }
    } // 개체 선택 사용자 입력
}