using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private readonly FEM fem;
        private readonly Bck3D bckD;

        Shape pointMarker;

        public MainWindow()
        {
            InitializeComponent();

            dkpStructureConcrete.Visibility = Visibility.Collapsed;
            dkpCameraControl.Visibility = Visibility.Collapsed;
            dkpFemWorks.Visibility = Visibility.Collapsed;

            smr = new SMR();
            fem = new FEM();
            bckD = new Bck3D(grdMain);

            TurnMouseEventGrdMain3dScroll(true);

        }


        private void StartAddNode(object sender, RoutedEventArgs e)
        {
            
            MouseMove += ShowPointMarkerByMouseMove;
            MouseDown += AddNodeByMouseLeftDown;
            KeyUp += EndAddNodeByEsc;

            bckD.RedrawShapes();
        }

        private void EndAddNodeByEsc(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                bckD.pointMarker.Hide();
                MouseMove -= ShowPointMarkerByMouseMove;
                MouseDown -= AddNodeByMouseLeftDown;
                KeyUp -= EndAddNodeByEsc;
                bckD.RedrawShapes();
            }
        }

        private void AddNodeByMouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton==MouseButtonState.Pressed)
            {
                Point3D p3d = bckD.Get3dPiontByMousePosition(e.GetPosition(grdMain));
                fem.model.nodes.Add(p3d);
                RedrawFemModel();
            }
        }

        private void ShowPointMarkerByMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            bckD.pointMarker.visibility = true;
            Point p = e.GetPosition(grdMain);
            Point3D p3d = bckD.pointMarker.Position(p);
            stbLabel.Content = "Add Node at (" + p3d.X + ", " + p3d.Y + ", " + p3d.Z + ")";
            bckD.RedrawShapes();
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

        private void DrawLine(object sender, RoutedEventArgs e)
        {
            bckD.DrawLine(1, 200, 2, 300, 8);
        }
        private void DrawSampleGradient(object sender, RoutedEventArgs e)
        {
            bckD.DrawSampleGradient();
        }
        private void DrawCone(object sender, RoutedEventArgs e)
        {
            double radius = 5;
            Point3D center = new Point3D(-1, -1, -1);
            Vector3D heightVector = new Vector3D(10,0, 0);
            //bckD.DrawCone(center, radius, heightVector, resolution, Colors.AliceBlue);
            bckD.shapes.AddCone(radius, heightVector, center, 6);
            bckD.RedrawShapes();
        }
        private void Draw3dLine(object sender, RoutedEventArgs e)
        {
            Point3D sp = new Point3D(0, 0, 0);
            Point3D epX = new Point3D(10, 0, 0);
            Point3D epY = new Point3D(0, 10, 0);
            Point3D epZ = new Point3D(0, 0, 10);

            //bckD.Draw3dLine(sp, epX);
            //bckD.Draw3dLine(sp, epY);
            //bckD.Draw3dLine(sp, epZ);

            bckD.shapes.AddLine(sp, epX);
            bckD.shapes.AddLine(sp, epY);
            bckD.shapes.AddLine(sp, epZ);
            bckD.RedrawShapes();

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
            bckD.shapes.AddCylinderClosed(str, dir, dia, resolution);
            bckD.shapes.recentShape.Color(Colors.Red);


            dir = new Vector3D(0, len, 0);
            bckD.shapes.AddCylinderClosed(str, dir, dia, resolution);
            bckD.shapes.recentShape.Color(Colors.Green);

            dir = new Vector3D(0, 0, len);
            //bckD.DrawCylinderClosed(str, dir, dia, resolution, Colors.Black);
            bckD.shapes.AddCylinderClosed(str, dir, dia, resolution);
            bckD.shapes.recentShape.Color(Colors.Black);
            //bckD.shapes.AddBox(new Point3D(0, 0, 0), new Vector3D(10, 10, 10));
            bckD.RedrawShapes();
        }
        private void DrawSphere(object sender, RoutedEventArgs e)
        {
            Point3D point = new Point3D(0, 0, 0);
            double diameter = 5;
            int resolution = 48;

            bckD.shapes.AddSphere(point, diameter, resolution);
            bckD.shapes.recentShape.Color(Colors.Red);
            bckD.RedrawShapes();
        }
        private void PerformanceTest(object sender, RoutedEventArgs e)
        {
            for (int i = 1; i < 20; i++)
            {
                for (int j = 1; j < 20; j++)
                {
                    for (int k = 1; k < 20; k++)
                    {
                        bckD.shapes.AddCylinderClosed(new Point3D(i, j, k), new Vector3D(0.5, 0, 0), 0.2, 16);
                        bckD.shapes.recentShape.Color(Colors.Magenta);
                    }
                }
            }
            bckD.RedrawShapes();
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

    }
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
            tbxCameraPositionX.Text = bckD.MyPCamera.Position.X.ToString();
            tbxCameraPositionY.Text = bckD.MyPCamera.Position.Y.ToString();
            tbxCameraPositionZ.Text = bckD.MyPCamera.Position.Z.ToString();
            tbxCameraLookDirectionX.Text = bckD.MyPCamera.LookDirection.X.ToString();
            tbxCameraLookDirectionY.Text = bckD.MyPCamera.LookDirection.Y.ToString();
            tbxCameraLookDirectionZ.Text = bckD.MyPCamera.LookDirection.Z.ToString();
            tbxCameraUpDirectionX.Text = bckD.MyPCamera.UpDirection.X.ToString();
            tbxCameraUpDirectionY.Text = bckD.MyPCamera.UpDirection.Y.ToString();
            tbxCameraUpDirectionZ.Text = bckD.MyPCamera.UpDirection.Z.ToString();
            tbxCameraFarPlaneDistance.Text = bckD.MyPCamera.FarPlaneDistance.ToString();
            tbxCameraFieldOfView.Text = bckD.MyPCamera.FieldOfView.ToString();
            tbxCameraNearPlaneDistance.Text = bckD.MyPCamera.NearPlaneDistance.ToString();
        }
        private void SetCameraInfo(object sender, RoutedEventArgs e)
        {
            bckD.MyPCamera.Position = new Point3D
            {
                X = Convert.ToDouble(tbxCameraPositionX.Text),
                Y = Convert.ToDouble(tbxCameraPositionY.Text),
                Z = Convert.ToDouble(tbxCameraPositionZ.Text)
            };
            bckD.MyPCamera.LookDirection = new Vector3D
            {
                X = Convert.ToDouble(tbxCameraLookDirectionX.Text),
                Y = Convert.ToDouble(tbxCameraLookDirectionY.Text),
                Z = Convert.ToDouble(tbxCameraLookDirectionZ.Text)
            };
            bckD.MyPCamera.UpDirection = new Vector3D
            {
                X = Convert.ToDouble(tbxCameraUpDirectionX.Text),
                Y = Convert.ToDouble(tbxCameraUpDirectionY.Text),
                Z = Convert.ToDouble(tbxCameraUpDirectionZ.Text)
            };
            bckD.MyPCamera.FieldOfView = Convert.ToDouble(tbxCameraFieldOfView.Text);
            bckD.MyPCamera.NearPlaneDistance = Convert.ToDouble(tbxCameraNearPlaneDistance.Text);
            bckD.MyPCamera.FarPlaneDistance = Convert.ToDouble(tbxCameraFarPlaneDistance.Text);
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
            bckD.shapes.AddBox(new Point3D(0, 0, 0), new Vector3D(smr.structure.length, smr.structure.width, smr.structure.height));
            bckD.RedrawShapes();
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
            elems.CountElems();
            if (elems.countTruss != 0) item.Items.Add(new TreeViewItem() { Header = "Truss(" + elems.countTruss + ")" });
            if (elems.countFrame != 0) item.Items.Add(new TreeViewItem() { Header = "Frame(" + elems.countFrame + ")" });
            if (elems.countCable != 0) item.Items.Add(new TreeViewItem() { Header = "Cable(" + elems.countCable + ")" });
            if (elems.countPlate != 0) item.Items.Add(new TreeViewItem() { Header = "Plate(" + elems.countPlate + ")" });
            if (elems.countSolid != 0) item.Items.Add(new TreeViewItem() { Header = "Solid(" + elems.countSolid + ")" });
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
            bckD.showCoordinateSystem = sd.IsChecked;
            bckD.RedrawShapes();
        }
        private void ViewBasePlaneGrid(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem sd = (System.Windows.Controls.MenuItem)sender;
            bckD.showBasePlaneGrid = sd.IsChecked;
            bckD.RedrawShapes();
        }

        public void RedrawFemModel()
        {
            RedrawFemWorksTreeView();

            bool showUndeformedShape = true;
            bool showSection = true;

            double maxLoadLength = 2;
            double maxLoadSize = fem.loads.GetMaxLoadLength();
            double loadViewScale = maxLoadLength / maxLoadSize;

            bckD.shapes.Clear();
            bckD.texts.Clear();

            double diaNode;
            double diaElem;
            int rlsNode;
            int rlsElem;
            Color colorNode;
            Color colorElem;
            Color colorLoad;
            Color colorReaction;

            diaNode = 0.15;
            diaElem = 0.05;
            rlsNode = 12;
            rlsElem = 6;
            colorNode = Colors.Red;
            colorElem = Colors.Blue;
            colorLoad = Colors.Red;
            colorReaction = Colors.Yellow;

            //Deformation
            if (fem.solved)
            {
                if (fem.model.nodes.show)
                {
                    foreach (Node node in fem.model.nodes)
                    {
                        bckD.shapes.AddSphere(node.c1, diaNode, rlsNode);
                        bckD.shapes.recentShape.Color(colorNode);
                    }
                }
                if (fem.model.nodes.showNumber)
                {
                    foreach (Node node in fem.model.nodes)
                    {
                        bckD.texts.Add(node.num.ToString(), node.c1, 8);
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
                                    bckD.shapes.AddPolygon(str, dir, frame.section.poly);
                                }
                                else
                                {
                                    bckD.shapes.AddCylinder(str, dir, diaElem, rlsElem);
                                }
                                break;
                            case 40:
                                Plate p = (Plate)e;
                                bckD.shapes.AddBox(p.nodes[0].c1, p.nodes[2].c1 - p.nodes[0].c1);

                                break;
                            case 80:
                                Solid s = (Solid)e;
                                bckD.shapes.AddHexahedron(s.nodes[0].c1, s.nodes[1].c1, s.nodes[2].c1, s.nodes[3].c1, s.nodes[4].c1, s.nodes[5].c1, s.nodes[6].c1, s.nodes[7].c1);
                                break;
                            default:
                                bckD.shapes.recentShape.Color(colorElem);
                                break;
                        }
                    }
                }
                if (fem.model.elems.showNumber)
                {
                    foreach (Element elem in fem.model.elems)
                    {
                        bckD.texts.Add(elem.num.ToString(), elem.Center, 8);
                    }
                }

                foreach (FemLoad load in fem.loads)
                {
                    foreach (NodalLoad nodalLoad in load.nodalLoads)
                    {
                        bckD.shapes.AddForce(nodalLoad.node.c1, nodalLoad.force * loadViewScale);
                        bckD.shapes.recentShape.Color(colorLoad);
                    }
                }

                //Reaction Force
                foreach (Node node in fem.model.nodes)
                {
                    Vector3D dir = new Vector3D(node.reactionForce[0], node.reactionForce[1], node.reactionForce[2]);
                    bckD.shapes.AddForce(node.c1, dir * loadViewScale);
                    bckD.shapes.recentShape.Color(colorReaction);
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
                            bckD.shapes.AddSphere(node.c0, diaNode, rlsNode);
                            bckD.shapes.recentShape.Color(colorNode);
                            bckD.shapes.recentShape.Opacity(opacity);
                        }
                    }
                    if (fem.model.nodes.showNumber)
                    {
                        foreach (Node node in fem.model.nodes)
                        {
                            bckD.texts.Add(node.num.ToString(), node.c0, 8);
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

                                if (showSection & frame.section.hasSectionPoly)
                                {
                                    bckD.shapes.AddPolygon(str, dir, frame.section.poly);
                                }
                                else
                                {
                                    bckD.shapes.AddCylinder(str, dir, diaElem, rlsElem);
                                }
                                break;
                            case 40:
                                Plate p = (Plate)e;
                                bckD.shapes.AddBox(p.nodes[0].c0, p.nodes[2].c0 - p.nodes[0].c0);

                                break;
                            case 80:
                                Solid s = (Solid)e;
                                bckD.shapes.AddHexahedron(s.nodes[0].c0, s.nodes[1].c0, s.nodes[2].c0, s.nodes[3].c0, s.nodes[4].c0, s.nodes[5].c0, s.nodes[6].c0, s.nodes[7].c0);
                                break;
                            default:
                                break;
                        }
                        bckD.shapes.recentShape.Color(colorElem);
                        bckD.shapes.recentShape.Opacity(opacity);
                    }
                }
                if (showUndeformedForce)
                {
                    foreach (FemLoad load in fem.loads)
                    {
                        foreach (NodalLoad nodalLoad in load.nodalLoads)
                        {
                            bckD.shapes.AddForce(load.nodalLoads[0].node.c0, nodalLoad.force * loadViewScale);
                            bckD.shapes.recentShape.Color(colorLoad);
                            bckD.shapes.recentShape.Opacity(opacity);
                        }
                    }
                }
            }

            bckD.RedrawShapes();
        }
        private void RedrawShapes(object sender, RoutedEventArgs e)
        {
            bckD.RedrawShapes();
        }

    } // 패널 표시여부, 그리드 및 좌표계 표시여부 등
    public partial class MainWindow : Window
    {
        private Point pointMouseDown;

        private void TurnMouseEventGrdMain3dScroll(bool on)
        {
            if (on)
            {
                this.MouseDown += new MouseButtonEventHandler(MainGridMouseDown);
                this.MouseUp += new MouseButtonEventHandler(MainGridMouseUp);
                this.MouseWheel += new MouseWheelEventHandler(MainGridMouseWheel);
                grdMain.MouseLeave += new System.Windows.Input.MouseEventHandler(MinGridMouseLeave);
            }
            else
            {
            }
        }
        private void MainGridMouseWheel(object sender, MouseWheelEventArgs e)
        {
            bckD.OrbitForward(e.Delta);
            GetCameraInfo();
        }
        private void MainGridMouseUp(object sender, MouseButtonEventArgs e)
        {
            //stbLabel.Content = (p.X) + ", " + (p.Y);
            stbLabel.Content = "";
            bckD.OrbitEnd();
            this.MouseMove -= new System.Windows.Input.MouseEventHandler(MainGridMouseMove_Orbit);
        }
        private void MinGridMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            stbLabel.Content = "";
        }
        private void MainGridMouseMove_Orbit(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point p = e.GetPosition(grdMain);
                Vector mov = p - pointMouseDown;
                stbLabel.Content = mov.X + ", " + mov.Y;

                bckD.OrbitRotate(mov);
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
                bckD.OrbitTwist(rad, dist);
                GetCameraInfo();
            }
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                Point p = e.GetPosition(grdMain);
                Vector mov = p - pointMouseDown;
                stbLabel.Content = mov.X + ", " + mov.Y;
                //bckD.OrbitMoveX(mov.X / 2); //MoveX와 MoveY중 처음 실행된 것 하나만 동작함.
                //bckD.OrbitMoveY(mov.Y / 2);
                bckD.OrbitMove(mov);
                GetCameraInfo();
            }
        }
        private void MainGridMouseDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            pointMouseDown = e.GetPosition(grdMain);
            bckD.OrbitStart();
            stbLabel.Content = "0, 0";
            this.MouseMove += new System.Windows.Input.MouseEventHandler(MainGridMouseMove_Orbit);

            if (e.LeftButton == MouseButtonState.Pressed)
            {

            }
        }
    } // 마우스 이벤트 관련
    public partial class MainWindow : Window
    {
        private void ViewSW(object sender, RoutedEventArgs e)
        {
            bckD.ViewSW();
        }
        private void ViewSE(object sender, RoutedEventArgs e)
        {
            bckD.ViewSE();
        }
        private void ViewNW(object sender, RoutedEventArgs e)
        {
            bckD.ViewNW();
        }
        private void ViewNE(object sender, RoutedEventArgs e)
        {
            bckD.ViewNE();
        }
        private void ViewTop(object sender, RoutedEventArgs e)
        {
            bckD.ViewTop();
        }
        private void ViewFront(object sender, RoutedEventArgs e)
        {
            bckD.ViewFront();
        }
        private void ViewBottom(object sender, RoutedEventArgs e)
        {
            bckD.ViewBottom();
        }
        private void ViewLeft(object sender, RoutedEventArgs e)
        {
            bckD.ViewLeft();
        }
        private void ViewRight(object sender, RoutedEventArgs e)
        {
            bckD.ViewRight();
        }
        private void ViewBack(object sender, RoutedEventArgs e)
        {
            bckD.ViewBack();
        }
        private void ViewZoomExtend(object sender, RoutedEventArgs e)
        {
            bckD.ViewZoomExtend();
        }

    } // View 컨트롤

}

