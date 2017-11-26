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





        private void deleteVertice_Click(object sender, RoutedEventArgs e)
        {
            context.currentPolyline.deleteVertice(target);
            context.visuals.RefreshPolyline(context.currentPolyline);
            this.Close();
        }


    }
}
