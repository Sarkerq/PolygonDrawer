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
            if (target.state == EdgeState.Horizontal)
            {
                forceHorizontal.Content = "Unset horizontal";
            }
            if (target.state == EdgeState.Vertical)
            {
                forceVertical.Content = "Unset vertical";

            }
        }

        private void newVertice_Click(object sender, RoutedEventArgs e)
        {
            context.drawnPolygon.PutVerticeInTheMiddle(target);
            context.RefreshPolygon(context.drawnPolygon);

            Close();
        }

        private void forceVertical_Click(object sender, RoutedEventArgs e)
        {
            if (target.state == EdgeState.Vertical)
            {
                target.ClearStatus();
                context.RefreshPolygon(context.drawnPolygon);
            }
            else
            {
                if(!context.drawnPolygon.canForce(target, EdgeState.Vertical))
                {
                    MessageBox.Show("You can't force two consecutive edges to be vertical or set vertical to fixed angle vertice!", "Settings error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                target.ClearStatus();

                target.ForceVertical();
                context.RepairAndRefreshPolygon(context.drawnPolygon, target);
            }
            Close();
        }

        private void forceHorizontal_Click(object sender, RoutedEventArgs e)
        {
            if (target.state == EdgeState.Horizontal)
            {
                target.ClearStatus();
                context.RefreshPolygon(context.drawnPolygon);
            }
            else
            {
                if (!context.drawnPolygon.canForce(target, EdgeState.Horizontal))
                {
                    MessageBox.Show("You can't force two consecutive edges to be horizontal or set horizontal to fixed angle vertice!", "Settings error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                target.ClearStatus();

                target.ForceHorizontal();
                context.RepairAndRefreshPolygon(context.drawnPolygon, target);
            }
            Close();

        }
    }
}
