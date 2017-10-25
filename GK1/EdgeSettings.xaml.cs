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
namespace GK1
{
    /// <summary>
    /// Interaction logic for EdgeSettings.xaml
    /// </summary>
    public partial class EdgeSettings : Window
    {
        Edge target;
        MainWindow context;
        public EdgeSettings(MainWindow _context, Edge _target)
        {
            InitializeComponent();
            target = _target;
            context = _context;
        }

        private void newVertice_Click(object sender, RoutedEventArgs e)
        {
            context.putVerticeInTheMiddle(target);
            context.repairAndRedrawPolygon(context.drawnPolygon);

            this.Close();
        }

        private void forceVertical_Click(object sender, RoutedEventArgs e)
        {
            context.forceVertical(target);
            context.repairAndRedrawPolygon(context.drawnPolygon);

            this.Close();
        }

        private void forceHorizontal_Click(object sender, RoutedEventArgs e)
        {
            context.forceHorizontal(target);
            context.repairAndRedrawPolygon(context.drawnPolygon);

            this.Close();

        }
    }
}
