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
            switch (caller)
            {
                case Caller.Texture:
                    mainWindow.currentTexture = sender as Image;
                    ((BitmapImage)(mainWindow.currentTexture.Source)).CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    if (mainWindow.constantTextureColor.IsChecked == true)
                        mainWindow.constantTextureColor_Checked(null, null);
                    break;
                case Caller.Map:

                    mainWindow.currentMap = sender as Image;
                    ((BitmapImage)(mainWindow.currentMap.Source)).CreateOptions = BitmapCreateOptions.IgnoreImageCache;

                    if (mainWindow.normalVectorMap.IsChecked == true)
                        mainWindow.normalVectorMap_Checked(null, null);
                    break;
                case Caller.Disturbance:

                    mainWindow.currentDisturbance = sender as Image;
                    ((BitmapImage)(mainWindow.currentDisturbance.Source)).CreateOptions = BitmapCreateOptions.IgnoreImageCache;

                    if (mainWindow.lightMapDisturbance.IsChecked == true)
                        mainWindow.lightMapDisturbance_Checked(null, null);
                    break;
            }

            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (caller)
            {
                case Caller.Texture:
                    loadFromFileTo(mainWindow.currentTexture);
                    if (mainWindow.constantTextureColor.IsChecked == true)
                        mainWindow.constantTextureColor_Checked(null, null);
                    break;
                case Caller.Map:

                    loadFromFileTo(mainWindow.currentMap);
                    if (mainWindow.normalVectorMap.IsChecked == true)
                        mainWindow.normalVectorMap_Checked(null, null);
                    break;
                case Caller.Disturbance:

                    loadFromFileTo(mainWindow.currentDisturbance);
                    if (mainWindow.lightMapDisturbance.IsChecked == true)
                        mainWindow.lightMapDisturbance_Checked(null, null);
                    break;
            }

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
