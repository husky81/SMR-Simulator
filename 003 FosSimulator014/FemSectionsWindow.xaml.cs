using BCK.SmrSimulation.Draw2D;
using BCK.SmrSimulation.finiteElementMethod;
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
using System.Windows.Shapes;

namespace bck.SmrSimulator.main
{
    /// <summary>
    /// Interaction logic for FemSectionsWindow.xaml
    /// </summary>
    public partial class FemSectionsWindow : Window
    {
        FemSectionCollection sections;
        BckDraw2D draw2d;

        public FemSectionsWindow(FemSectionCollection sections)
        {
            InitializeComponent();
            this.sections = sections;
            draw2d = new BckDraw2D(grdSectionView);

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Point p0 = new Point(0, 0);
            Point p1 = new Point(10, 10);
            draw2d.shapes.lines.Add(p0,p1);
            draw2d.RedrawShapes();


        }
    }
}
