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
        BitmapSource bitmap;
        public Carbon visuals = new Carbon();
        public GKPolygon drawnPolygon;
        Edge currentlyedgeDragged;
        Vector mouseToVertice1;
        Vector mouseToVertice2;
        bool newPolygonMode = true;
        private Point dragWholePolygonCoords;
        private bool polygonDragged;
        bool dragged = false;
        bool edgeDragged = false;
        int currentlyDragged = 0;



        public MainWindow()
        {
            InitializeComponent();
            drawnPolygon = new GKPolygon(visuals);

            UpdateScreen();

        }

        internal void RepairAndRefreshPolygon(GKPolygon drawnPolygon, Vertice target)
        {
            int index = drawnPolygon.vertices.IndexOf(target);
            RepairAndRefreshPolygon(drawnPolygon, index);
        }

        void UpdateScreen()
        {
            bitmap = BitmapSource.Create(visuals.width, visuals.height,
                96, 96, visuals.pf, null, visuals.pixelData, visuals.rawStride);
            lineCarbon.Source = bitmap;
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
                else
                {
                    OnMovedPolygon(e);
                }

            }
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
            return visuals.pixelOwner[xIndex + yIndex];
        }

        private void OnNewVertice(Point location)
        {
            Vertice newV = new Vertice(location);
            if (newPolygonMode && drawnPolygon.vertices.Count >= 1)
            {
                drawnPolygon.edges.Add(new Edge(drawnPolygon.vertices.Last(), newV));
            }
            drawnPolygon.vertices.Add(newV);
            RefreshPolygon(drawnPolygon);
        }

        private void OnClickedVertice(Vertice target)
        {
            if (newPolygonMode)
            {
                if (target == drawnPolygon.vertices.First())
                {
                    drawnPolygon.edges.Add(new Edge(drawnPolygon.vertices.Last(), drawnPolygon.vertices.First()));
                    newPolygonMode = false;
                }

            }
            else
            {
                dragged = true;
                currentlyDragged = drawnPolygon.vertices.IndexOf(target);
            }
            RefreshPolygon(drawnPolygon);
        }

        private Vertice ClickedVertice(Point location)
        {
            foreach (Vertice v in drawnPolygon.vertices)
            {
                if (Math.Abs(v.coords.X - location.X) <= Global.verticeRadius && Math.Abs(v.coords.Y - location.Y) <= Global.verticeRadius)
                {
                    return v;
                }
            }
            return null;
        }




        public void RepairAndRefreshPolygon(GKPolygon polygon, int changedVerticeIndex)
        {
            polygon.RepairVertices(changedVerticeIndex);
            RefreshPolygon(polygon);
            UpdateScreen();
        }
        public void RefreshPolygon(GKPolygon polygon)
        {
            visuals.redrawPolygon(polygon);
            UpdateScreen();
        }



        public void RepairAndRefreshPolygon(GKPolygon polygon, Edge changedEdge)
        {

            polygon.RepairVertices(changedEdge);
            RefreshPolygon(drawnPolygon);
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
            dragged = false;
            drawnPolygon = new GKPolygon(visuals);
            RefreshPolygon(drawnPolygon);
        }



        private void drawingScreen_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (dragged)
            {
                drawnPolygon.vertices[currentlyDragged].coords.X = e.GetPosition(drawingScreen).X;
                drawnPolygon.vertices[currentlyDragged].coords.Y = e.GetPosition(drawingScreen).Y;
                RepairAndRefreshPolygon(drawnPolygon, currentlyDragged);
                dragged = false;
                return;
            }
            if (edgeDragged)
            {
                currentlyedgeDragged.v1.coords.X = Vector.Add(-mouseToVertice1, Mouse.GetPosition(drawingScreen)).X;
                currentlyedgeDragged.v1.coords.Y = Vector.Add(-mouseToVertice1, Mouse.GetPosition(drawingScreen)).Y;
                currentlyedgeDragged.v2.coords.X = Vector.Add(-mouseToVertice2, Mouse.GetPosition(drawingScreen)).X;
                currentlyedgeDragged.v2.coords.Y = Vector.Add(-mouseToVertice2, Mouse.GetPosition(drawingScreen)).Y;
                RepairAndRefreshPolygon(drawnPolygon, currentlyedgeDragged);
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

        private void DraggingPolygon(Point pt)
        {
            Vector displacement = pt - dragWholePolygonCoords;
            foreach (Vertice v in drawnPolygon.vertices)
            {
                v.coords += displacement;
            }
            RefreshPolygon(drawnPolygon);
            dragWholePolygonCoords = pt;
        }

        private void drawingScreen_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragged)
            {
                drawnPolygon.vertices[currentlyDragged].coords.X = Mouse.GetPosition(drawingScreen).X;
                drawnPolygon.vertices[currentlyDragged].coords.Y = Mouse.GetPosition(drawingScreen).Y;
                RepairAndRefreshPolygon(drawnPolygon, currentlyDragged);
                return;
            }
            if (edgeDragged)
            {
                currentlyedgeDragged.v1.coords.X = Vector.Add(-mouseToVertice1, Mouse.GetPosition(drawingScreen)).X;
                currentlyedgeDragged.v1.coords.Y = Vector.Add(-mouseToVertice1, Mouse.GetPosition(drawingScreen)).Y;
                currentlyedgeDragged.v2.coords.X = Vector.Add(-mouseToVertice2, Mouse.GetPosition(drawingScreen)).X;
                currentlyedgeDragged.v2.coords.Y = Vector.Add(-mouseToVertice2, Mouse.GetPosition(drawingScreen)).Y;
                RepairAndRefreshPolygon(drawnPolygon, currentlyedgeDragged);

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
            Vertice targetVertice = ClickedVertice(e.GetPosition(drawingScreen));
            if (targetVertice != null) OnRightClickedVertice(targetVertice);
            else
            {
                Edge targetEdge = ClickedEdge(e.GetPosition(drawingScreen));
                if (targetEdge != null) OnRightClickedEdge(targetEdge, e);
                else if (newPolygonMode) FinishPolygon();
            }

        }

        private void FinishPolygon()
        {
            if (drawnPolygon.vertices.Count < 3)
            {
                MessageBox.Show("Polygon should have at least 3 vertices!", "Not enough vertices", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            drawnPolygon.edges.Add(new Edge(drawnPolygon.vertices.Last(), drawnPolygon.vertices.First()));
            RefreshPolygon(drawnPolygon);
            newPolygonMode = false;
        }

        private void OnRightClickedEdge(Edge targetEdge, MouseButtonEventArgs e)
        {
            ShowModifyEdgeWindow(targetEdge);
        }

        private void ShowModifyEdgeWindow(Edge modificationTarget)
        {
            EdgeSettings window = new EdgeSettings(this, modificationTarget);
            window.Show();
        }

        private void OnRightClickedVertice(Vertice targetVertice)
        {
            ShowSetAngleWindow(targetVertice);
        }

        private void ShowSetAngleWindow(Vertice modificationTarget)
        {
            SetAngle window = new SetAngle(this, modificationTarget);
            window.Show();
        }


    }
    public enum VerticeFix
    {
        Left,
        Right
    }
    public enum EdgeState
    {
        Horizontal,
        Vertical,
        None
    }



}
