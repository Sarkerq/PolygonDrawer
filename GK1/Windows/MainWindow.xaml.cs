using GK1.Windows;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GK1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        public List<GKPolygon> polygons;
        public GKPolygon currentPolygon;
        public GKPolygon clippingPolygon;

        public Carbon visuals;
        Edge currentlyedgeDragged;
        Vector mouseToVertice1;
        Vector mouseToVertice2;
        ApplicationMode mode;
        private Point dragWholePolygonCoords;
        private bool polygonDragged;
        bool dragged = false;
        bool edgeDragged = false;
        int currentlyDragged = 0;
        ImageSource texture;


        public MainWindow()
        {

            InitializeComponent();
            visuals = new Carbon(lineCarbon);

            mode = ApplicationMode.NewPolygon;
            currentPolygon = new GKPolygon(visuals);
            polygons = new List<GKPolygon>();
            polygons.Add(currentPolygon);

            constantLightsourceVector_Checked(null, null);

            constantObjectColor_Checked(null, null);

            constantNormalVector_Checked(null, null);

            noDisturbance_Checked(null, null);
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(160000);
            dispatcherTimer.Start();

            visuals.UpdateScreen();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (visuals.lightsourceRadius > 0 && animatedLightsourceVector.IsChecked == true)
            {
                visuals.lightPixelVector[0] += Carbon.MAX_WIDTH / Carbon.TICKS_PER_DAY;
                if (visuals.lightPixelVector[0] > Carbon.MAX_WIDTH) visuals.lightPixelVector[0] -= Carbon.MAX_WIDTH;
                visuals.lightPixelVector[1] = Carbon.MAX_HEIGHT / 2;
                visuals.lightPixelVector[2] =
                    visuals.lightsourceRadius *
                    (1 - Math.Abs(visuals.lightPixelVector[0] - Carbon.MAX_WIDTH / 2) / (Carbon.MAX_WIDTH / 2));
                RefreshAllPolygons(polygons);
            }
        }

        private void OnNewVertice(Point location)
        {
            Vertice newV = new Vertice(location);
            if (mode == ApplicationMode.NewPolygon && currentPolygon.vertices.Count >= 1)
            {
                currentPolygon.edges.Add(new Edge(currentPolygon.vertices.Last(), newV));
            }
            currentPolygon.vertices.Add(newV);
            RefreshAllPolygons(polygons);
        }
        private void OnMovedPolygon(MouseButtonEventArgs e)
        {
            dragWholePolygonCoords = e.GetPosition(drawingScreen);
            polygonDragged = true;
        }

        private void OnClickedEdge(Edge targetEdge, MouseButtonEventArgs e)
        {
            mouseToVertice1 = new Vector(e.GetPosition(drawingScreen).X - targetEdge.v1.coords.X,
                                         e.GetPosition(drawingScreen).Y - targetEdge.v1.coords.Y);
            mouseToVertice2 = new Vector(e.GetPosition(drawingScreen).X - targetEdge.v2.coords.X,
                                         e.GetPosition(drawingScreen).Y - targetEdge.v2.coords.Y);

            edgeDragged = true;
            currentlyedgeDragged = targetEdge;
        }

        private Edge ClickedEdge(Point point)
        {
            int xIndex = (int)point.X * 3;
            int yIndex = (int)point.Y * visuals.rawStride;
            if (currentPolygon.edges.Contains(visuals.pixelOwner[xIndex + yIndex]))
                return visuals.pixelOwner[xIndex + yIndex];
            else
                return null;
        }



        private void OnClickedVertice(Vertice target)
        {
            if (mode == ApplicationMode.NewPolygon)
            {
                if (target == currentPolygon.vertices.First())
                {
                    currentPolygon.edges.Add(new Edge(currentPolygon.vertices.Last(), currentPolygon.vertices.First()));
                    endNewPolygonMode();
                }

            }

            else if (currentPolygon.vertices.Contains(target))
            {
                dragged = true;
                currentlyDragged = currentPolygon.vertices.IndexOf(target);
            }
            RefreshAllPolygons(polygons);
        }

        private void endNewPolygonMode()
        {
            mode = ApplicationMode.Standard;
            addPolygon.IsEnabled = true;
            clipPolygon.IsEnabled = true;
        }
        private void endAddPolygonMode()
        {
            mode = ApplicationMode.Standard;
        }
        private Vertice ClickedVertice(Point location)
        {

            foreach (Vertice v in currentPolygon.vertices)
            {
                if (Math.Abs(v.coords.X - location.X) <= Global.verticeRadius && Math.Abs(v.coords.Y - location.Y) <= Global.verticeRadius)
                {
                    return v;
                }
            }
            return null;
        }




        public void RefreshPolygon(GKPolygon polygon)
        {
            if (polygon.edges.Count == polygon.vertices.Count && polygon.edges.Count > 0)
            {

                visuals.fillPolygon(polygon);
            }
            if (mode == ApplicationMode.ClipPolygon && polygon == clippingPolygon)
                visuals.redrawClippingPolygon(polygon);
            else if (polygon == currentPolygon)
                visuals.redrawCurrentPolygon(polygon);
            else
                visuals.redrawPolygon(polygon);


        }
        public void RefreshAllPolygons(List<GKPolygon> polygons)
        {
            visuals.clear();
            foreach (GKPolygon p in polygons)
            {
                RefreshPolygon(p);
            }
            visuals.UpdateScreen();

        }

        private void ClearCanvas()
        {
            drawingScreen.Children.Clear();
            drawingScreen.Children.Add(lineCarbon);
        }


        private void newPolygon_Click(object sender, RoutedEventArgs e)
        {

            ClearCanvas();
            visuals.clear();
            mode = ApplicationMode.NewPolygon;
            addPolygon.IsEnabled = false;
            clipPolygon.IsEnabled = false;
            dragged = false;
            currentPolygon = new GKPolygon(visuals);
            polygons = new List<GKPolygon>();
            polygons.Add(currentPolygon);
            RefreshAllPolygons(polygons);
        }




        private void DraggingPolygon(Point pt)
        {
            Vector displacement = pt - dragWholePolygonCoords;
            foreach (Vertice v in currentPolygon.vertices)
            {
                v.coords += displacement;
            }
            RefreshAllPolygons(polygons);
            dragWholePolygonCoords = pt;
        }

        private void drawingScreen_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragged)
            {

                currentPolygon.vertices[currentlyDragged].coords.X = Mouse.GetPosition(drawingScreen).X;
                currentPolygon.vertices[currentlyDragged].coords.Y = Mouse.GetPosition(drawingScreen).Y;
                RefreshAllPolygons(polygons);
                return;
            }
            if (edgeDragged)
            {

                currentlyedgeDragged.v1.coords.X = Vector.Add(-mouseToVertice1, Mouse.GetPosition(drawingScreen)).X;
                currentlyedgeDragged.v1.coords.Y = Vector.Add(-mouseToVertice1, Mouse.GetPosition(drawingScreen)).Y;
                currentlyedgeDragged.v2.coords.X = Vector.Add(-mouseToVertice2, Mouse.GetPosition(drawingScreen)).X;
                currentlyedgeDragged.v2.coords.Y = Vector.Add(-mouseToVertice2, Mouse.GetPosition(drawingScreen)).Y;
                RefreshAllPolygons(polygons);

                return;
            }
            if (polygonDragged)
            {
                DraggingPolygon(e.GetPosition(drawingScreen));
                return;
            }

        }

        private void drawingScreen_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (dragged)
            {
                currentPolygon.vertices[currentlyDragged].coords.X = e.GetPosition(drawingScreen).X;
                currentPolygon.vertices[currentlyDragged].coords.Y = e.GetPosition(drawingScreen).Y;
                RefreshAllPolygons(polygons);
                dragged = false;
                return;
            }
            if (edgeDragged)
            {

                currentlyedgeDragged.v1.coords.X = Vector.Add(-mouseToVertice1, Mouse.GetPosition(drawingScreen)).X;
                currentlyedgeDragged.v1.coords.Y = Vector.Add(-mouseToVertice1, Mouse.GetPosition(drawingScreen)).Y;
                currentlyedgeDragged.v2.coords.X = Vector.Add(-mouseToVertice2, Mouse.GetPosition(drawingScreen)).X;
                currentlyedgeDragged.v2.coords.Y = Vector.Add(-mouseToVertice2, Mouse.GetPosition(drawingScreen)).Y;
                RefreshAllPolygons(polygons);
                edgeDragged = false;
                return;
            }
            if (polygonDragged)
            {
                DraggingPolygon(e.GetPosition(drawingScreen));
                polygonDragged = false;
                return;
            }
        }




        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            Vertice targetVertice = ClickedVertice(e.GetPosition(drawingScreen));
            if (targetVertice != null) OnClickedVertice(targetVertice);
            else
            {
                Edge targetEdge = ClickedEdge(e.GetPosition(drawingScreen));
                if (targetEdge != null) OnClickedEdge(targetEdge, e);
                else if (mode == ApplicationMode.NewPolygon) OnNewVertice(e.GetPosition(drawingScreen));

                else
                {
                    OnMovedPolygon(e);
                }

            }
        }


        private void drawingScreen_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (mode == ApplicationMode.NewPolygon) FinishPolygon();
            else
            {
                Vertice targetVertice = ClickedVertice(e.GetPosition(drawingScreen));
                if (targetVertice != null) OnRightClickedVertice(targetVertice);
                else
                {
                    Edge targetEdge = ClickedEdge(e.GetPosition(drawingScreen));
                    if (targetEdge != null) OnRightClickedEdge(targetEdge, e);

                }
            }

        }
        private void drawingScreen_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void drawingScreen_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (mode == ApplicationMode.ClipPolygon)
            {
                int clippingPolygonIndex = polygons.IndexOf(clippingPolygon);

                if (e.Delta > 0)
                {
                    clippingPolygon = polygons[(clippingPolygonIndex + 1) % polygons.Count];
                }
                else
                {
                    clippingPolygon = polygons[(clippingPolygonIndex - 1 + polygons.Count) % polygons.Count];
                }
            }
            else
            {
                int currentPolygonIndex = polygons.IndexOf(currentPolygon);

                if (e.Delta > 0)
                {
                    currentPolygon = polygons[(currentPolygonIndex + 1) % polygons.Count];
                }
                else
                {
                    currentPolygon = polygons[(currentPolygonIndex - 1 + polygons.Count) % polygons.Count];
                }
            }
            RefreshAllPolygons(polygons);
        }

        private void FinishPolygon()
        {
            if (currentPolygon.vertices.Count < 3)
            {
                MessageBox.Show("Polygon should have at least 3 vertices!", "Not enough vertices", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            currentPolygon.edges.Add(new Edge(currentPolygon.vertices.Last(), currentPolygon.vertices.First()));
            RefreshAllPolygons(polygons);
            endNewPolygonMode();
        }

        private void OnRightClickedEdge(Edge targetEdge, MouseButtonEventArgs e)
        {
            ShowModifyEdgeWindow(targetEdge);
        }

        private void ShowModifyEdgeWindow(Edge modificationTarget)
        {
            EdgeSettings window = new EdgeSettings(this, modificationTarget);
            window.ShowDialog();
        }

        private void OnRightClickedVertice(Vertice targetVertice)
        {
            ShowSetAngleWindow(targetVertice);
        }

        private void ShowSetAngleWindow(Vertice modificationTarget)
        {
            SetAngle window = new SetAngle(this, modificationTarget);
            window.ShowDialog();
        }

        private void addPolygon_Click(object sender, RoutedEventArgs e)
        {
            currentPolygon = new GKPolygon(visuals);
            polygons.Add(currentPolygon);
            mode = ApplicationMode.NewPolygon;
            clipPolygon.IsEnabled = false;
            addPolygon.IsEnabled = false;

        }



        private void clipPolygon_Click(object sender, RoutedEventArgs e)
        {
            if (mode != ApplicationMode.ClipPolygon)
            {
                clippingPolygon = currentPolygon;
                mode = ApplicationMode.ClipPolygon;
                addPolygon.IsEnabled = false;
            }
            else
            {
                mode = ApplicationMode.Standard;
                addPolygon.IsEnabled = true;

            }
            RefreshAllPolygons(polygons);
        }

        private void drawingScreen_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.LeftShift:
                    {
                        if (mode == ApplicationMode.ClipPolygon)
                        {
                            if (clippingPolygon.IsConvex())

                            {
                                currentPolygon = SutherlandHodgman.GetIntersectedPolygon(currentPolygon, clippingPolygon);
                                if (currentPolygon.vertices.Count == 0)
                                {
                                    polygons.Remove(currentPolygon);
                                    currentPolygon = polygons[0];
                                }
                                RefreshAllPolygons(polygons);
                            }
                            else
                                MessageBox.Show("Clipping polygon must be convex!", "Bad polygon", MessageBoxButton.OK, MessageBoxImage.Exclamation);

                        }
                        break;
                    }
                default:
                    break;

            }
        }



        private void constantObjectColor_Checked(object sender, RoutedEventArgs e)
        {
            if (visuals != null)
            {
                visuals.texturePixelData = new byte[3];
                if (ClrPcker_Object != null && ClrPcker_Object.SelectedColor != null)
                {
                    visuals.texturePixelData[0] = ((Color)(ClrPcker_Object.SelectedColor)).R;
                    visuals.texturePixelData[1] = ((Color)(ClrPcker_Object.SelectedColor)).G;
                    visuals.texturePixelData[2] = ((Color)(ClrPcker_Object.SelectedColor)).B;

                }
                else
                {
                    visuals.texturePixelData[0] = 255;
                    visuals.texturePixelData[1] = 255;
                    visuals.texturePixelData[2] = 255;
                }
                visuals.texturePixelDataWidth = 1;
                visuals.texturePixelDataHeight = 1;
                if (polygons != null)
                    RefreshAllPolygons(polygons);
            }
        }

        private void constantTextureColor_Checked(object sender, RoutedEventArgs e)
        {
            if (currentTexture != null)
            {
                visuals.texturePixelData = Global.ImageSourceToBytes(currentTexture.Source);

                visuals.texturePixelDataWidth = ((BitmapImage)(currentTexture.Source)).PixelWidth;
                visuals.texturePixelDataHeight = ((BitmapImage)(currentTexture.Source)).PixelHeight;
            }
            if (polygons != null)
                RefreshAllPolygons(polygons);
        }

        private void normalVectorMap_Checked(object sender, RoutedEventArgs e)
        {
            if (currentMap != null)
            {
                visuals.mapPixelData = Global.ImageSourceToBytes(currentMap.Source);

                visuals.mapPixelDataWidth = ((BitmapImage)(currentMap.Source)).PixelWidth;
                visuals.mapPixelDataHeight = ((BitmapImage)(currentMap.Source)).PixelHeight;
            }
            if (polygons != null)
                RefreshAllPolygons(polygons);
        }

        private void constantNormalVector_Checked(object sender, RoutedEventArgs e)
        {
            if (visuals != null)
            {
                visuals.mapPixelData = new byte[3];

                visuals.mapPixelData[0] = 127;
                visuals.mapPixelData[1] = 127;
                visuals.mapPixelData[2] = 255;
                visuals.mapPixelDataWidth = 1;
                visuals.mapPixelDataHeight = 1;
                if (polygons != null)
                    RefreshAllPolygons(polygons);
            }
            if (polygons != null)
                RefreshAllPolygons(polygons);
        }

        private void constantLightsourceVector_Checked(object sender, RoutedEventArgs e)
        {
            if (visuals != null)
            {
                visuals.lightsourceRadius = 0;
                visuals.lightPixelVector = new double[3];
                visuals.lightPixelVector[0] = 0;
                visuals.lightPixelVector[1] = 0;
                visuals.lightPixelVector[2] = 1;
                if (polygons != null)
                    RefreshAllPolygons(polygons);
            }
        }

        private void animatedLightsourceVector_Checked(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(lightsourceRadius.Text, out visuals.lightsourceRadius))
            {
                MessageBox.Show("Please input correct number!", "Bad input", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (polygons != null)
                RefreshAllPolygons(polygons);
        }

        private void noDisturbance_Checked(object sender, RoutedEventArgs e)
        {
            if (visuals != null)
            {
                visuals.disturbancePixelData = new byte[3];
                if (ClrPcker_Object != null && ClrPcker_Object.SelectedColor != null)
                {
                    visuals.disturbancePixelData[0] = ((Color)(ClrPcker_Object.SelectedColor)).R;
                    visuals.disturbancePixelData[1] = ((Color)(ClrPcker_Object.SelectedColor)).G;
                    visuals.disturbancePixelData[2] = ((Color)(ClrPcker_Object.SelectedColor)).B;

                }
                else
                {
                    visuals.disturbancePixelData[0] = 255;
                    visuals.disturbancePixelData[1] = 255;
                    visuals.disturbancePixelData[2] = 255;
                }
                visuals.disturbancePixelDataWidth = 1;
                visuals.disturbancePixelDataHeight = 1;
                if (polygons != null)
                    RefreshAllPolygons(polygons);
            }
            if (polygons != null)
                RefreshAllPolygons(polygons);
        }

        private void lightMapDisturbance_Checked(object sender, RoutedEventArgs e)
        {
            if (currentDisturbance != null)
            {
                visuals.disturbancePixelData = Global.ImageSourceToBytes(currentDisturbance.Source);

                visuals.disturbancePixelDataWidth = ((BitmapImage)(currentDisturbance.Source)).PixelWidth;
                visuals.disturbancePixelDataHeight = ((BitmapImage)(currentDisturbance.Source)).PixelHeight;
            }
            if (polygons != null)
                RefreshAllPolygons(polygons);
        }

        private void ClrPcker_Object_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (constantObjectColor.IsChecked == true)
            {
                constantObjectColor_Checked(null, null);
            }
        }



        private void ClrPcker_Lightsource_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (e.NewValue != null)
            {
                visuals.lightsourceColor = (Color)e.NewValue;
                constantLightsourceVector_Checked(null, null);

            }
        }

        private void lightsourceRadius_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (visuals != null)
            {
                if (!double.TryParse(lightsourceRadius.Text, out visuals.lightsourceRadius))
                {
                    MessageBox.Show("Please input correct number!", "Bad input", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void currentTexture_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            loadFromFileTo(currentTexture);
            if (constantTextureColor.IsChecked == true)
                constantTextureColor_Checked(null, null);

        }
        private void currentMap_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            loadFromFileTo(currentMap);
            if (normalVectorMap.IsChecked == true)
                normalVectorMap_Checked(null, null);

        }

        private void currentDisturbance_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            loadFromFileTo(currentDisturbance);
            if (lightMapDisturbance.IsChecked == true)
                lightMapDisturbance_Checked(null, null);


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

    enum ApplicationMode
    {
        Standard,
        NewPolygon,
        AddPolygon,
        ClipPolygon
    }



}
