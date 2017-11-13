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
        public Carbon visuals;
        Edge currentlyedgeDragged;
        Vector mouseToVertice1;
        Vector mouseToVertice2;
        bool newPolygonMode = true;
        bool addPolygonMode = false;
        bool addPolygonModeFinished = false;
        private Point dragWholePolygonCoords;
        private bool polygonDragged;
        bool dragged = false;
        bool edgeDragged = false;
        int currentlyDragged = 0;



        public MainWindow()
        {
            InitializeComponent();
            visuals = new Carbon(lineCarbon);
            currentPolygon = new GKPolygon(visuals);
            polygons = new List<GKPolygon>();
            polygons.Add(currentPolygon);
            visuals.UpdateScreen();

        }





        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            Vertice targetVertice = ClickedVertice(e.GetPosition(drawingScreen));
            if (targetVertice != null) OnClickedVertice(targetVertice);
            else
            {
                Edge targetEdge = ClickedEdge(e.GetPosition(drawingScreen));
                if (targetEdge != null) OnClickedEdge(targetEdge, e);
                else if (newPolygonMode) OnNewVertice(e.GetPosition(drawingScreen));



            }
        }
        private void OnNewVertice(Point location)
        {
            Vertice newV = new Vertice(location);
            if (newPolygonMode && currentPolygon.vertices.Count >= 1)
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
            int xIndex = (int)point.X * 4;
            int yIndex = (int)point.Y * visuals.rawStride;
            if (currentPolygon.edges.Contains(visuals.pixelOwner[xIndex + yIndex]))
                return visuals.pixelOwner[xIndex + yIndex];
            else
                return null;
        }



        private void OnClickedVertice(Vertice target)
        {
            if (newPolygonMode)
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
            newPolygonMode = false;
            addPolygon.IsEnabled = true;
        }
        private void endAddPolygonMode()
        {
            addPolygonMode = false;
            addPolygonModeFinished = true;
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
            if (polygon == currentPolygon)
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
            newPolygonMode = true;
            addPolygon.IsEnabled = false;
            addPolygonModeFinished = false;
            addPolygonMode = false;

            dragged = false;
            currentPolygon = new GKPolygon(visuals);
            polygons = new List<GKPolygon>();
            polygons.Add(currentPolygon);
            RefreshAllPolygons(polygons);
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




        private void drawingScreen_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (newPolygonMode) FinishPolygon();
            else if (polygonDragged)
            {
                DraggingPolygon(e.GetPosition(drawingScreen));
                polygonDragged = false;
                return;
            }
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
            newPolygonMode = true;
        }

        private void drawingScreen_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

            OnMovedPolygon(e);
        }

        private void drawingScreen_MouseWheel(object sender, MouseWheelEventArgs e)
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
            RefreshAllPolygons(polygons);
        }
    }





}
