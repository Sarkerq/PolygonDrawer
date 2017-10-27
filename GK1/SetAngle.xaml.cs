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
    /// Interaction logic for SetAngle.xaml
    /// </summary>
    public partial class SetAngle : Window
    {
        MainWindow context;
        Vertice target;
        public SetAngle(MainWindow _context, Vertice _target)
        {
            InitializeComponent();
            target = _target;
            context = _context;
            if (target.fixedAngle) unsetAngle.Visibility = Visibility.Visible;
        }

        private void confirm_Click(object sender, RoutedEventArgs e)
        {
            if (!context.drawnPolygon.canSetAngle(target))
            {
                MessageBox.Show("You can't fix angle with neighbouring fixed horizontal or vertical edge!", "Settings error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                double angle = ParseDouble(degrees.Text) * Math.PI / 180;
                context.drawnPolygon.ForceAngle(context.drawnPolygon.previousVertice(target), target, context.drawnPolygon.nextVertice(target), angle );
                context.RepairAndRefreshPolygon( context.drawnPolygon, target);

                this.Close();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show("Angle should be a number!", "Wrong input", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private double ParseDouble(string text)
        {
            if (double.TryParse(text, out double parsedDouble))
                return parsedDouble;
            else
                throw new ArgumentException();

        }



        private void deleteVertice_Click(object sender, RoutedEventArgs e)
        {
            context.drawnPolygon.deleteVertice(target);
            context.RefreshPolygon(context.drawnPolygon);
            this.Close();
        }

        private void unsetAngle_Click(object sender, RoutedEventArgs e)
        {
            target.fixedAngleValue = 0;
            target.fixedAngle = false;
            context.RefreshPolygon(context.drawnPolygon);
            this.Close();
        }
    }
}
