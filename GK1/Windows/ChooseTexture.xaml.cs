using Microsoft.Win32;
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

namespace GK1.Windows
{
    /// <summary>
    /// Interaction logic for ChooseTexture.xaml
    /// </summary>
    public partial class ChooseTexture : Window
    {
        private MainWindow mainWindow;
        private Caller caller;

        public ChooseTexture(MainWindow mainWindow, Caller caller)
        {
            InitializeComponent();

            this.mainWindow = mainWindow;
            this.caller = caller;
        }

        private void currentTexture_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }


        private void loadFromFileTo(Image img)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png";
            if (op.ShowDialog() == true)
            {
                img.Source = new BitmapImage(new Uri(op.FileName));
            }
        }
    }
    public enum Caller
    {
        Texture,
        Map,
        Disturbance
    }
}
