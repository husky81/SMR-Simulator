using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace _003_FosSimulator014
{
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
        private readonly string initialCmdMark = "Command";
        private readonly string cmdMark = ": ";

        private string userInput;
        private Point3D userInputPoint3D;
        private Vector3D userInputVector3D;
        private double userInputDouble;
        private int userInputInt;
        private InputTypes requestedInputType = InputTypes.None;

        internal delegate void inputInt(int n);
        internal inputInt actionAfterIntWithInt;
        internal delegate void inputDirInt(Vector3D dir, int n);
        internal inputDirInt actionAfterIntWithDirInt;
        internal delegate void inputDir(Vector3D dir);
        internal inputDir actionAfterDirWithDir;
        

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
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Point p0 = e.GetPosition(main.grdMain);
                    mouseDownSubCommand.runMouseDown(p0);
                }
            }
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
            //명령창에서 사용자가 입력한 명령어 반환
            string userInput = GetCommandTextFromCommandWindowText();
            userInput = userInput.ToUpper(); //대문자로 변경

            //사용자가 명령어 없이 스페이스만 누른 경우 처리
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

                if (main.requestUserInput != null)
                {
                    if (main.requestUserInput.On) //이미 명령이 실행중인 경우 다시 실행
                    {
                        Enter();
                        main.requestUserInput.DoAction();
                        return;
                    }
                }

                WriteText("(last command) " + lastCommand.name);
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
                case InputTypes.None:
                    break;
                case InputTypes.Int:
                    if (userInputType == requestedInputType)
                    {
                        if (actionAfterIntWithInt != null)
                        {
                            actionAfterIntWithInt(userInputInt);
                            actionAfterIntWithInt = null;
                        }
                    }
                    return;
                case InputTypes.Double:
                    break;
                case InputTypes.Point:
                    break;
                case InputTypes.Direction:
                    if (userInputType == InputTypes.Direction)
                    {
                        if (actionAfterDirWithDir != null)
                        {
                            actionAfterDirWithDir(userInputVector3D);
                            actionAfterDirWithDir = null;
                            return;
                        }
                    }
                    if (userInputType == InputTypes.Point)
                    {
                        main.requestUserInput.PutDirectionFirstPoint(userInputPoint3D);
                        return;
                    }
                    else
                    {
                        //상대좌표를 요청했는데 입력되지 않은 경우.
                        WriteText(" (상대좌표값이 아닙니다. 상대좌표를 입력하세요. ex: @0,1 or @0,1,0)");
                        Enter();
                        main.requestUserInput.DoAction();
                        return;
                    }
                    break;
                case InputTypes.Dist:
                    break;
                default:
                    break;
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

            InputTypes GetTypeOfUserInput(string uInp)
            {
                //입력값이 상대좌표인 경우. @로 시작하는 경우 상대좌표로 인식.
                if (uInp.Substring(0, 1).Equals("@"))
                {
                    int isRelativeCoordinateInput = IsRelativeCoordinateInput(uInp.Substring(1));
                    if (isRelativeCoordinateInput >= 0) Enter();
                    switch (isRelativeCoordinateInput)
                    {
                        case 2:
                            if (main.requestUserInput.On)
                            {
                                main.requestUserInput.Put(userInputVector3D);
                            }
                            return InputTypes.Direction;
                        case 3:
                            if (main.requestUserInput.On)
                            {
                                main.requestUserInput.Put(userInputVector3D);
                            }
                            return InputTypes.Direction;
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
                            if (main.requestUserInput.On)
                            {
                                main.requestUserInput.Put(userInputInt);
                            }
                            return InputTypes.Int;
                        case 1:
                            if (main.requestUserInput.On)
                            {
                                main.requestUserInput.Put(userInputDouble);
                            }
                            return InputTypes.Double;
                        case 2:
                            if (main.requestUserCoordinatesInput.on)
                            {
                                main.requestUserCoordinatesInput.Put(userInputPoint3D);
                            }
                            return InputTypes.Point;
                        case 3:
                            if (main.requestUserCoordinatesInput.on)
                            {
                                main.requestUserCoordinatesInput.Put(userInputPoint3D);
                            }
                            return InputTypes.Point; ;
                        default:
                            break;
                    }

                }
                return InputTypes.None;
            }

        }

        private Command FindCommandFromUserInput(string userInput)
        {
            //입력 명령어와 동일한 명령 실행
            foreach (Command cmd in activeCommand.commands)
            {
                if (userInput.Equals(cmd.shortName.ToUpper()))
                {
                    return cmd;
                }
                if (userInput.Equals(cmd.name.ToUpper()))
                {
                    return cmd;
                }
            }
            return null;
        }

        private string GetCommandTextFromCommandWindowText()
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
            if (main.requestUserInput == null)
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

        internal void RequestInput_Int(string message)
        {
            WriteText(message + cmdMark);
            SetCursorLast();
            requestedInputType = InputTypes.Int;
        }

        internal void RequestInput_Direction(string message)
        {
            WriteText(message + cmdMark);
            SetCursorLast();
            requestedInputType = InputTypes.Direction;
            main.MouseDown += GetDirection;
        }
        private void GetDirection(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
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
                main.requestUserInput.Put(inputDirection);
            }
        }

        private Point3D GetPoint3dFromPoint2D(Point p0)
        {
            return main.draw.GetPoint3dOnBasePlane_FromPoint2D(p0);
        }
        private Point GetPointFromPoint3D(Point3D p3d)
        {
            return main.draw.GetPoint2D_FromPoint3D(p3d);
        }
        enum InputTypes
        {
            None,
            Int,
            Double,
            Point,
            Direction,
            Dist
        }
    } //명령창 명령어 관리

}
