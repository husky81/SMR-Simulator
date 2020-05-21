using bck.SMR_simulator.draw3d;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CanvasTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Draw3D draw = new Draw3D(mainGrid);

            Point3D p0 = new Point3D(0, 0, 0);
            Point3D p1 = new Point3D(10, 0, 0);

            draw.shapes.AddLine(p0, p1);
            draw.shapes.AddSphere(p0, 10, 12);

            draw.IsOnZoomPan_WheelScroll = true;


            draw.RegenerateShapes_ModelVisual3ds();
            draw.RedrawShapes();

            draw.ViewTop();


        }
    }
}
