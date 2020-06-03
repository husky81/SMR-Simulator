using bck.SMR_simulator.main.Properties;
using BCK.SmrSimulation.Draw2D;
using BCK.SmrSimulation.Draw3D;
using BCK.SmrSimulation.GeneralFunctions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace BCK.SmrSimulation.Main
{
    public class CommandWindow
    {
        readonly MainWindow main;
        readonly System.Windows.Controls.TextBox textBox;

        /// <summary>
        /// 모든 command의 상위 command, root command.
        /// </summary>
        readonly Command rootCommand;
        Command activeCommand;
        Command lastCommand;
        readonly string initialCmdMark = "Command";
        readonly string cmdMark = ": ";
        bool isMouseInput = false;

        public CommandWindow(MainWindow mainWindow, System.Windows.Controls.TextBox textBox)
        {
            main = mainWindow;
            Command.main = mainWindow;
            this.textBox = textBox;

            rootCommand = new Command("Main");
            activeCommand = rootCommand;

            Clear();
            SetCommandStructure();

            //textBox가 non인 경우가 발생할 수 있으므로 경고 옵션을 꺼둠.
            #pragma warning disable CA1062 // Validate arguments of public methods
            textBox.PreviewKeyDown += Tbx_PreviewKeyDown; //space, enter, backspace 처리
            #pragma warning restore CA1062 // Validate arguments of public methods
            
            textBox.KeyDown += Tbx_KeyDown; //esc 처리
            textBox.KeyUp += Tbx_KeyUp; //space, enter 후처리
        }
        internal class Command
        {
            internal static MainWindow main;
            internal delegate void Run();
            internal Run run; //하위 Command가 있든 없든 무조건 실행
            /// <summary>
            /// 이미 선택된 개체가 있는 경우 실행
            /// </summary>
            internal Run runSelected;

            internal delegate void RunMouse(Point p0);
            internal RunMouse runAfterMouseDown; //하위명령 커리 중 마우스를 클릭하는 경우 실행
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
                    if (cmd.runAfterMouseDown != null)
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
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Point p0 = e.GetPosition(main.grdMain);
                    mouseDownSubCommand.runAfterMouseDown(p0);
                }
            }
        }

        void SetCommandStructure()
        {
            Command cmd, subCmd;
            cmd = rootCommand.Add("Regen", "re"); cmd.run += main.RedrawFemModel;
            cmd = rootCommand.Add("Redraw", "r"); cmd.run += main.RedrawShapes;

            cmd = rootCommand.Add("Zoom", "z");
            subCmd = cmd.Add("All", "a"); subCmd.run += main.ZoomAll;
            subCmd = cmd.Add("Extents", "e"); subCmd.run += main.ZoomExtents;
            subCmd = cmd.Add("Window", "w"); subCmd.run += main.ZoomWindow;

            cmd = rootCommand.Add("Circle", "ci"); subCmd.run += main.DrawCicleCenterRadius;
            subCmd = cmd.Add("Radius", "r"); subCmd.run += main.DrawCicle;

            cmd = rootCommand.Add("Erase", "e"); cmd.runSelected += main.EraseSelected;
            subCmd = cmd.Add("All", "a"); subCmd.run += main.EraseAll;
            subCmd = cmd.Add("Fence", "f"); subCmd.run += main.EraseFence;

            cmd = rootCommand.Add("Line", "l"); cmd.run += main.AddFemLine;

            cmd = rootCommand.Add("Divide", "d"); cmd.run += main.DivideElem;

            cmd = rootCommand.Add("Extrude", "ex"); cmd.run += main.ExtrudeElem;

            cmd = rootCommand.Add("Select", "s");
            subCmd = cmd.Add("Node", "n"); subCmd.run += main.SelectNode;
            subCmd = cmd.Add("Element", "e"); subCmd.run += main.SelectElem;

            cmd = rootCommand.Add("Boundary", "b");
            subCmd = cmd.Add("FixAll", "f"); subCmd.run += main.BoundaryFixAll;
            subCmd = cmd.Add("FixDXYZ", "fd"); subCmd.run += main.BoundaryFixDXYZ;
            subCmd = cmd.Add("DX",  "x"); subCmd.run += main.BoundaryFixDx;
            subCmd = cmd.Add("DY",  "y"); subCmd.run += main.BoundaryFixDy;
            subCmd = cmd.Add("DZ",  "z"); subCmd.run += main.BoundaryFixDz;
            subCmd = cmd.Add("RX", "rx"); subCmd.run += main.BoundaryFixRx;
            subCmd = cmd.Add("RY", "ry"); subCmd.run += main.BoundaryFixRy;
            subCmd = cmd.Add("RZ", "rz"); subCmd.run += main.BoundaryFixRz;
            subCmd = cmd.Add("Remove", "r"); subCmd.run += main.BoundaryRemove;

            cmd = rootCommand.Add("Force", "f"); cmd.runSelected += main.FemAddLoadSelected;

            cmd = rootCommand.Add("Move", "m"); cmd.runSelected += main.FemMoveSelected;


        } //명령어 구성!!!

        /// <summary>
        /// 명령창에 입력된 명령어 인식 및 처리. 명령창에서 스페이스를 누르면이 이 함수가 실행됨.
        /// 사용자가 입력한 명령 실행. 커멘드 창에서 space, enter를 누르면 실행됨.
        /// </summary>
        void GetCommand()
        {
            //명령창에서 사용자가 입력한 명령어 반환
            string userInput = GetCommandTextFromCommandWindowText();

            userInput = userInput.ToUpper(main.CultureInfo); //대문자로 변경

            //사용자가 명령어 없이 스페이스만 누른 경우 처리
            if (userInput.Length==0)
            {
                if (lastCommand == null)
                {
                    //WriteText("직전에 실행한 명령이 없습니다.");
                    Enter();
                    NewLine();
                    return;
                }
                if (main.requestUserInput != null)
                {
                    if (main.requestUserInput.IsOn) //이미 명령이 실행중인 경우 다시 실행
                    {
                        if (requestedInputType == InputTypes.Points)
                        {
                            //Points를 요청하는 절점의 끝을 입력한 경우.
                            Enter();
                            PutPoints_End();
                            //NewLine();
                            return;
                        }
                        else
                        {
                            Enter();
                            main.requestUserInput.DoAction();
                            return;
                        }
                    }
                }

                WriteText(lastCommand.name + " (last command)");
                Enter();
                ExecuteCommand(lastCommand); // 직전 명령 다시 실행
                return;
            }

            //사용자가 입력한 command가 있는 경우 실행
            Command cmd = FindCommandFromUserInput(userInput);
            if (cmd != null)
            {
                WriteText(" " + cmd.name);
                Enter();
                ExecuteCommand(cmd);
                return;
            }

            //사용자가 입력한 값의 형식에 따라서 구분
            InputTypes userInputType = GetTypeOfUserInput(userInput);
            switch (requestedInputType)
            {
                case InputTypes.Int:
                    if (userInputType == requestedInputType)
                    {
                        if (actionAfterIntWithInt != null)
                        {
                            actionAfterIntWithInt(userInputInt);
                            actionAfterIntWithInt = null;
                            EndCommand();
                        }
                        return;
                    }
                    break;
                case InputTypes.Ints:
                    switch (userInputType)
                    {
                        case InputTypes.Int:
                            userInputInts = new List<int>();
                            userInputInts.Add(userInputInt);
                            actionAfterIntsWithInts(userInputInts);
                            EndCommand();
                            return;
                        case InputTypes.Ints:
                            actionAfterIntsWithInts(userInputInts);
                            NewLine();
                            EndCommand();
                            return;
                        default:
                            break;
                    }
                    break;
                case InputTypes.Double:
                    break;
                case InputTypes.Point:
                    break;
                case InputTypes.Vector:
                case InputTypes.VectorValue:
                    switch (userInputType)
                    {
                        case InputTypes.Point: //상대좌표를 요청했는데 절대좌표가 입력된 경우.
                            if (requestedInputType == InputTypes.VectorValue & !isMouseInput)
                            {
                                //VectorValue는 키보드로 절대좌표를 입력한 경우 입력값을 그대로 벡터로 반환함.
                                userInputVector3D = userInputPoint3D - new Point3D(0, 0, 0);
                                ActionAfterVecWithVec();
                            }
                            else
                            {
                                PutVectorPoint(userInputPoint3D);
                            }
                            return;
                        case InputTypes.Vector:
                            ActionAfterVecWithVec();
                            return;
                        default:
                            break;
                    }
                    //상대좌표를 요청했는데 입력되지 않은 경우.
                    WriteText(" (상대좌표값이 아닙니다. 상대좌표를 입력하세요. ex: @0,1 or @0,1,0)");
                    Enter();
                    main.requestUserInput.DoAction();
                    return;
                case InputTypes.Dist:
                    break;
                case InputTypes.Points:
                    switch (userInputType)
                    {
                        case InputTypes.Point:
                            PutPoints_Point(userInputPoint3D);
                            return;
                        case InputTypes.Vector:
                            PutPoints_Direction(userInputVector3D);
                            return;
                        case InputTypes.Int:
                            PutPoints_MouseDirectionDist(userInputInt);
                            break;
                        default:
                            break;
                    }
                    break;
                case InputTypes.None:
                    break;
                default:
                    break;
            }

            //명령어가 없는 경우.
            WriteText(" Unknown command.");
            Enter();
            if (activeCommand == rootCommand)
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

        void ActionAfterVecWithVec()
        {
            if (actionAfterVecWithVec != null)
            {
                actionAfterVecWithVec(userInputVector3D);
                actionAfterVecWithVec = null;
                EndCommand();
            }
        }

        InputTypes GetTypeOfUserInput(string uInp)
        {
            //입력값이 상대좌표인 경우. @로 시작하는 경우 상대좌표로 인식.
            if (uInp.Substring(0, 1).Equals("@",StringComparison.CurrentCultureIgnoreCase))
            {
                int isRelativeCoordinateInput = IsRelativeCoordinateInput(uInp.Substring(1));
                if (isRelativeCoordinateInput >= 0) Enter();
                switch (isRelativeCoordinateInput)
                {
                    case 2:
                        return InputTypes.Vector;
                    case 3:
                        return InputTypes.Vector;
                    default:
                        break;
                }
            }

            //입력값이 좌표이거나 double, int 인 경우
            int isCoordinateInput = IsCoordinateInput(uInp);
            if (isCoordinateInput >= 0)
            {
                Enter();

                switch (isCoordinateInput)
                {
                    case 0:
                        return InputTypes.Int;
                    case 1:
                        return InputTypes.Double;
                    case 2:
                        return InputTypes.Point;
                    case 3:
                        return InputTypes.Point; ;
                    default:
                        break;
                }

            }

            //입력값이 1~5,10-15 형태의 List<int>표현인 경우.
            bool isIntList = IsIntList(uInp);
            if (isIntList)
            {
                return InputTypes.Ints;
            }

            return InputTypes.None;
        }
        bool IsIntList(string uInp)
        {
            userInputInts = GF.ConvertStringsToIntList(uInp);
            if (userInputInts == null) return false;
            return true;
        }

        string GetCommandTextFromCommandWindowText()
        {
            //커맨드 창에 입력된 명령어 추출
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
            return userInput;
        }
        Command FindCommandFromUserInput(string userInput)
        {
            //입력 명령어와 동일한 명령 실행
            foreach (Command cmd in activeCommand.commands)
            {
                if (userInput.Equals(cmd.shortName.ToUpper(main.CultureInfo)))
                {
                    return cmd;
                }
                if (userInput.Equals(cmd.name.ToUpper(main.CultureInfo)))
                {
                    return cmd;
                }
            }
            return null;
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
            int firstCommaLocation = userInput.IndexOf(",", StringComparison.CurrentCultureIgnoreCase);
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
                firstValue = double.Parse(firstString, main.CultureInfo.NumberFormat);
            }
            catch (Exception)
            {
                return falseResult;
            }

            string restString = userInput.Substring(firstCommaLocation + 1);

            int secondCommaLocation = restString.IndexOf(",", StringComparison.CurrentCultureIgnoreCase);
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

            secondString = restString.Substring(0, secondCommaLocation);
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

        void ExecuteCommand(Command cmd)
        {
            lastCommand = cmd;

            main.IsOnOrbit = false;

            if (main.Fem.Selection.Count > 0 & cmd.runSelected != null) //선택된 개체가 있고, cmd.runSelected를 지정한 경우.
            {
                WriteText("선택된 개체의 " + cmd.name + "을(를) 실행합니다.");
                Enter();
                cmd.runSelected();
                EndCommand();
                return;
            }
            if (cmd.run != null) //cmd.run이 지정된 경우
            {
                cmd.run();
                EndCommand();
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
        void EndCommand()
        {

            if (main.requestUserInput == null)
            {
                SetForOtherCommand();
                return;
            }
            if (!main.requestUserInput.IsOn)
            {
                SetForOtherCommand();
            }

            return;
            void SetForOtherCommand()
            {
                RemoveEvents_All();
                ClearActions();
                activeCommand = rootCommand;
                requestedInputType = InputTypes.None;
                
                NewLine();
            }
        }

        void Tbx_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    isMouseInput = false;
                    GetCommand();
                    break;
                case Key.Enter:
                    isMouseInput = false;
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
        void Tbx_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    if (activeCommand != rootCommand)
                    {
                        CancelByEsc();
                    }
                    else
                    {
                        CancelByEsc();
                    }
                    break;
            }
        }
        void Tbx_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
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

        private void CancelByEsc()
        {
            WriteText("*Cancel*");
            Enter();

            main.requestUserInput = new RequestUserInput(main);
            main.mouseInputGuide.End();
            main.mouseInputGuide3d.End();

            EndCommand();
        }
        void Clear()
        {
            textBox.Focus();
            textBox.Text = initialCmdMark + cmdMark;
            SetCursorLast();
        }
        void Space()
        {
            textBox.AppendText(" ");
            SetCursorLast();
        }
        void BackSpace(int length = 1)
        {
            textBox.Text = textBox.Text.Substring(0, textBox.Text.Length - length);
            SetCursorLast();
        }
        public void Enter()
        {
            textBox.AppendText(Environment.NewLine);
            SetCursorLast();
        }
        public void NewLine()
        {
            textBox.Focus();
            textBox.AppendText(initialCmdMark + cmdMark);
            SetCursorLast();
        }
        void WriteText(String text)
        {
            textBox.AppendText(text);
            SetCursorLast();
        }
        void SetCursorLast()
        {
            textBox.Select(textBox.Text.Length, 0);
        }
        public string GetLastLine()
        {
            string text = textBox.Text;
            string lastLineText = text.Split('\n').Last();
            return lastLineText;
        }

        public void SendRequestMessage(string message)
        {
            WriteText(message);
            WriteText(cmdMark);
        }
        internal void ErrorMessage(string v)
        {
            WriteText("Error!!! " + v);
            NewLine();
        }
        internal void WarningMessage(string v)
        {
            WriteText("Warning: " + v);
            NewLine();
        }
        public void Call(string cmdText)
        {
            WriteText(cmdText);
            SetCursorLast();
            GetCommand();
        } //외부에서 명령어 실행 요청

        //입력요청 관련
        InputTypes requestedInputType = InputTypes.None;
        string userInput;
        Point3D userInputPoint3D;
        Vector3D userInputVector3D;
        double userInputDouble;
        int userInputInt;
        List<int> userInputInts;
        List<Point3D> userInputPoints;

        internal delegate void inputInt(int n);
        internal inputInt actionAfterIntWithInt;
        internal delegate void inputInts(List<int> n);
        internal inputInts actionAfterIntsWithInts;
        internal delegate void inputDirInt(Vector3D dir, int n);
        internal inputDirInt actionAfterIntWithDirInt;
        internal delegate void inputDir(Vector3D dir);
        internal inputDir actionAfterVecWithVec;
        internal delegate void inputPoint(Point3D p0);
        internal inputPoint actionAfterPointWithPoint;
        internal delegate void inputPoints(List<Point3D> pointList);
        internal inputPoints actionAfterPoints;

        void ClearActions()
        {
            actionAfterIntWithInt = null;
            actionAfterIntWithDirInt = null;
            actionAfterVecWithVec = null;
            actionAfterPointWithPoint = null;
            actionAfterPoints = null;
        }

        /// <summary>
        /// 사용자에게 Int 값을 입력하도록 요청.
        /// </summary>
        /// <param name="message"></param>
        internal void RequestInput_Int(string message)
        {
            WriteText(message + cmdMark);
            SetCursorLast();
            requestedInputType = InputTypes.Int;
        }
        internal void RequestInput_Ints(string message)
        {
            WriteText(message + cmdMark);
            SetCursorLast();
            requestedInputType = InputTypes.Ints;
        }

        internal void RequestInput_Points(string message, int numPoint)
        {
            numPointRequested = numPoint;

            WriteText(message + cmdMark);
            SetCursorLast();
            requestedInputType = InputTypes.Points;
            userInputPoints = new List<Point3D>();

            main.MouseDown += GetPoints_Point;
            main.MouseMove += GetPoints_Moving;

        }
        int numPointRequested;
        void GetPoints_Point(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point p = e.GetPosition(main.grdMain);
                Point3D p3 = GetSnapAndOrthogonalPoint3dFromChangingPoint2d(ref p);
                main.Cmd.Call(p3.X + "," + p3.Y + "," + p3.Z);
            }
        }
        internal void PutPoints_Point(Point3D userInputPoint3D)
        {
            main.MouseDown -= GetPoints_Point;
            main.MouseMove -= GetPoints_Moving;
            main.mouseInputGuide3d.End();

            userInputPoints.Add(userInputPoint3D);
            actionAfterPointWithPoint?.Invoke(userInputPoint3D);

            if(numPointRequested == userInputPoints.Count)
            {
                PutPoints_End();
                return;
            }

            main.mouseInputGuide3d.viewType = MouseInputGuide3D.ViewType.Line;
            main.mouseInputGuide3d.Start(userInputPoint3D);
            main.MouseMove += GetPoints_Moving;

            WriteText("다음 점을 입력하세요" + cmdMark);
            SetCursorLast();

            main.MouseDown += GetPoints_Point;
        }
        private Point3D PreviousPoint
        {
            get
            {
                return userInputPoints[userInputPoints.Count - 1];
            }
        }
        void PutPoints_Direction(Vector3D userInputPoint3D)
        {
            Point3D nextPoint = PreviousPoint + userInputPoint3D;
            PutPoints_Point(nextPoint);
        }
        void PutPoints_MouseDirectionDist(int userInputInt)
        {
            Point3D previousPoint = PreviousPoint;
            Point3D currentMousePoint3D = GetSnapAndOrthogonalPoint3dFromChangingPoint2d(ref currentMousePoint);
            Vector3D mouseDirection = currentMousePoint3D - previousPoint;
            mouseDirection.Normalize();
            Point3D nextPoint = previousPoint + mouseDirection * userInputInt;
            PutPoints_Point(nextPoint);
        }
        private Point currentMousePoint;
        void GetPoints_Moving(object sender, MouseEventArgs e)
        {
            currentMousePoint = e.GetPosition(main.grdMain);

            //개체선택용 MouseInputGuide 클래스 사용 시 Cross 옵션인 경우 첫번째 점을 입력하기 전부터 시작되어야 함.
            if (viewType == MouseInputGuide.ViewType.Cross & !main.mouseInputGuide3d.started)
            {
                main.mouseInputGuide.viewType = viewType;
                main.mouseInputGuide.Start(currentMousePoint);
            }

            ObjectSnapPoint snapPoint = main.ChangeToSnapPointAndDrawMarkAndPutEvent(ref currentMousePoint, GetPoints_Point);
            if (snapPoint == null & Settings.Default.isOnOrthogonal) main.mouseInputGuide3d.GetOrthogonalPoint3dFromPoint2d(ref currentMousePoint);

            //Points 입력 요청일 때 2번째 입력값 부터 InputGuideLine생성.
            if (userInputPoints.Count > 0) main.mouseInputGuide3d.Move(currentMousePoint);
        }

        /// <summary>
        /// Point2D에 Snap 및 Orthogonal 옵션을 적용해서 변경된 절점으로 변경 및 Point3D 반환.
        /// 이때, ObjectSnapMark에 이벤트도 넣어줌.
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="eventObject"></param>
        /// <returns></returns>
        private Point3D GetSnapAndOrthogonalPoint3dFromChangingPoint2d(ref Point p0, MouseButtonEventHandler eventObject = null)
        {
            //마우스 위치를 SnapPoint로 변경. 마크 생성. 이번트 입력.
            ObjectSnapPoint snapPoint;
            if (eventObject == null)
            {
                snapPoint = main.ChangeToSnapPointAndDrawMark(ref p0);
            }
            else
            {
                snapPoint = main.ChangeToSnapPointAndDrawMarkAndPutEvent(ref p0, eventObject);
            }

            if (snapPoint == null)
            {
                //SnapPoint가 없는 경우
                if (Settings.Default.isOnOrthogonal)
                {
                    //수직선 옵션이 켜 있는 경우
                    return main.mouseInputGuide3d.GetOrthogonalPoint3dFromPoint2d(ref p0);
                }
                else
                {
                    //수직선 옵션이 꺼 있는 경우 그냥 대응하는 3차원 점 반환
                    return main.Draw.GetPoint3dOnBasePlaneFromPoint2D(p0);
                }
            }
            else
            {
                //SnapPoint가 있는 경우
                return snapPoint.point;
            }
        }

        void PutPoints_End()
        {
            main.mouseInputGuide3d.End();
            actionAfterPoints?.Invoke(userInputPoints);
            EndCommand();
        }
        void RemoveEvents_GetPoints()
        {
            main.MouseMove -= GetPoints_Moving;
            main.MouseDown -= GetPoints_Point;
        }

        /// <summary>
        /// 사용자에게 Vector3D 값을 입력하도록 요청. 마우스 혹은 키보드 입력 가능함.
        /// </summary>
        /// <param name="message"></param>
        internal void RequestInputVector(string message)
        {
            isSecondPointInputOfVector = false;
            WriteText(message + cmdMark);
            SetCursorLast();
            requestedInputType = InputTypes.Vector;
            main.MouseDown += GetVector;
            main.MouseMove += GetVector_Moving;
        }
        Point3D vectorFirstPoint;
        bool isSecondPointInputOfVector;
        /// <summary>
        /// 사용자에게 Vector3D 값을 입력하도록 요청. 키보드로 입력한 경우 값을 벡터로 반환. 마우스로 입력하는 경우 두번째 점을 요청하여 벡터 반환.
        /// </summary>
        /// <param name="message"></param>
        internal void RequestInputVectorValue(string message)
        {
            isSecondPointInputOfVector = false;
            WriteText(message + cmdMark);
            SetCursorLast();
            requestedInputType = InputTypes.VectorValue;
            main.MouseDown += GetVector;
        }
        private void GetVector(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point p = e.GetPosition(main.grdMain);
                Point3D p3 = GetPoint3dFromPointBySnapPoint(p);

                main.MouseDown -= GetVector;
                isMouseInput = true;
                main.Cmd.Call(p3.X + "," + p3.Y + "," + p3.Z);
                isSecondPointInputOfVector = true;
            }
        }
        internal void PutVectorPoint(Point3D userInputPoint3D)
        {
            if (isSecondPointInputOfVector)
            {
                RemoveEvents_GetVector();
                main.mouseInputGuide.End();

                Vector3D inputDirection = userInputPoint3D - vectorFirstPoint;
                main.requestUserInput.Put(inputDirection);
                EndCommand();
            }
            else
            {
                vectorFirstPoint = userInputPoint3D;
                Point p = GetPointFromPoint3D(userInputPoint3D);

                main.MouseMove += GetVector_Moving;
                main.mouseInputGuide.viewType = MouseInputGuide.ViewType.Line;
                main.mouseInputGuide.Start(p);

                WriteText("벡터의 방향을 입력하세요." + cmdMark);
                SetCursorLast();
                main.MouseDown += GetVector;
            }
        }
        void GetVector_Moving(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Point p0 = e.GetPosition(main.grdMain);
            main.ChangeToSnapPointAndDrawMarkAndPutEvent(ref p0, GetVector);
            main.mouseInputGuide.Move(p0);
        }
        void RemoveEvents_GetVector()
        {
            main.MouseDown -= GetVector;
            main.MouseMove -= GetVector_Moving;
        }

        internal MouseInputGuide.ViewType viewType;
        void RemoveEvents_All()
        {
            RemoveEvents_GetPoints();
            RemoveEvents_GetVector();
        }

        Point3D GetPoint3dFromPoint2D(Point p0)
        {
            return main.Draw.GetPoint3dOnBasePlaneFromPoint2D(p0);
        }
        Point GetPointFromPoint3D(Point3D p3d)
        {
            return main.Draw.GetPoint2DFromPoint3D(p3d);
        }
        /// <summary>
        /// ObjectSnap 처리. ObjctSnap이 켜져 있는 경우 입력 좌표를 ObjectSnap 좌표로 변경.
        /// </summary>
        /// <param name="p"></param>
        /// <returns>OsnapPoint 옵션을 고려한 3차원 좌표 반환</returns>
        Point3D GetPoint3dFromPointBySnapPoint(Point p)
        {
            main.ChangeToSnapPointAndDrawMark(ref p);
            Point3D p3;
            if (main.SnapPoint != null)
            {
                p3 = main.SnapPoint.point;
            }
            else
            {
                p3 = GetPoint3dFromPoint2D(p);
            }
            return p3;
        }

        enum InputTypes
        {
            None,
            Int,
            Ints,
            Double,
            Point,
            Dist,
            Points,
            /// <summary>
            /// 키보드로 절대좌표를 입력한 경우 다음점을 입력 요청.
            /// </summary>
            Vector,
            /// <summary>
            /// 키보드로 절대좌표를 입력한 경우 입력값을 그대로 벡터로 반환함.
            /// </summary>
            VectorValue
        }

        internal void GetCursor()
        {
            Keyboard.Focus(textBox);
            SetCursorLast();
        }
    } //명령창 명령어 관리

}
