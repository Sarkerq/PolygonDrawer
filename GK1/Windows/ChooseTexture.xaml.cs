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

        public ChooseTexture(MainWindow mainWindow)
        {
            InitializeComponent();

            this.mainWindow = mainWindow;
        }

        private void currentTexture_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mainWindow.currentImage = loadFromImageTo( sender as Image, mainWindow.currentImage);
            mainWindow.dispatcherTimer_Tick(null, null);
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            loadFromFileTo(mainWindow.currentImage);
            mainWindow.dispatcherTimer_Tick(null, null);
            this.Close();
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
                BitmapImage myBitmapImage = new BitmapImage();

                myBitmapImage.BeginInit();
                myBitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                myBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                myBitmapImage.UriSource = new Uri(op.FileName);

                myBitmapImage.EndInit();
                img.Source = null;
                img.Source = myBitmapImage;
            }
        }
        private Image loadFromImageTo(Image src, Image dst)
        {

            BitmapImage myBitmapImage = (src.Source as BitmapImage);   

            myBitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            myBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;

            dst.Source = null;
            dst.Source = myBitmapImage;
            return dst;
        }
    }
}
