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
        }

        private void confirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double angle = parseDouble(degrees.Text) * Math.PI / 180;
                context.forceAngle(target, angle, VerticeFix.Left);
                context.repairAndRedrawPolygon(context.drawnPolygon, target);

                this.Close();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show("Angle should be a number!", "Wrong input", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private double parseDouble(string text)
        {
            double parsedDouble;
            if (double.TryParse(text, out parsedDouble))
                return parsedDouble;
            else
                throw new ArgumentException();

        }
    }
}
