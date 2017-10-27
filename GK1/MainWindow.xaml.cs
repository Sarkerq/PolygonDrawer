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
        public GKPolygon auxiliaryPolygon;
        public GKPolygon oldPolygon;

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
            auxiliaryPolygon = new GKPolygon(visuals);
            drawnPolygon = new GKPolygon(visuals);
            oldPolygon = new GKPolygon(visuals);
            UpdateScreen();

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
                else if (addPolygonMode) OnNewAuxiliaryVertice(e.GetPosition(drawingScreen));

                else
                {
                    OnMovedPolygon(e);
                }

            }
        }
        private void OnNewVertice(Point location)
        {
            Vertice newV = new Vertice(location);
            if (newPolygonMode && drawnPolygon.vertices.Count >= 1)
            {
                drawnPolygon.edges.Add(new Edge(drawnPolygon.vertices.Last(), newV));
            }
            drawnPolygon.vertices.Add(newV);
            RefreshTwoPolygon(drawnPolygon, auxiliaryPolygon);
        }
        private void OnNewAuxiliaryVertice(Point location)
        {
            Vertice newV = new Vertice(location);
            if (addPolygonMode && auxiliaryPolygon.vertices.Count >= 1)
            {
                auxiliaryPolygon.edges.Add(new Edge(auxiliaryPolygon.vertices.Last(), newV));
            }
            auxiliaryPolygon.vertices.Add(newV);
            RefreshTwoPolygon(drawnPolygon, auxiliaryPolygon);
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
            oldPolygon = new GKPolygon(drawnPolygon);

            edgeDragged = true;
            currentlyedgeDragged = targetEdge;
        }

        private Edge ClickedEdge(Point point)
        {
            int xIndex = (int)point.X * 4;
            int yIndex = (int)point.Y * visuals.rawStride;
            if (drawnPolygon.edges.Contains(visuals.pixelOwner[xIndex + yIndex]))
                return visuals.pixelOwner[xIndex + yIndex];
            else
                return null;
        }



        private void OnClickedVertice(Vertice target)
        {
            if (newPolygonMode)
            {
                if (target == drawnPolygon.vertices.First())
                {
                    drawnPolygon.edges.Add(new Edge(drawnPolygon.vertices.Last(), drawnPolygon.vertices.First()));
                    endNewPolygonMode();
                }

            }
            else if (addPolygonMode)
            {
                if (target == auxiliaryPolygon.vertices.First())
                {
                    auxiliaryPolygon.edges.Add(new Edge(auxiliaryPolygon.vertices.Last(), auxiliaryPolygon.vertices.First()));
                    endAddPolygonMode();
                }
            }
            else if (drawnPolygon.vertices.Contains(target))
            {
                oldPolygon = new GKPolygon(drawnPolygon);
                dragged = true;
                currentlyDragged = drawnPolygon.vertices.IndexOf(target);
            }
            RefreshTwoPolygon(drawnPolygon, auxiliaryPolygon);
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

            foreach (Vertice v in drawnPolygon.vertices)
            {
                if (Math.Abs(v.coords.X - location.X) <= Global.verticeRadius && Math.Abs(v.coords.Y - location.Y) <= Global.verticeRadius)
                {
                    return v;
                }
            }
            foreach (Vertice v in auxiliaryPolygon.vertices)
            {
                if (Math.Abs(v.coords.X - location.X) <= Global.verticeRadius && Math.Abs(v.coords.Y - location.Y) <= Global.verticeRadius)
                {
                    return v;
                }
            }
            return null;
        }


        public void RepairAndRefreshBothPolygon(GKPolygon old, GKPolygon current, Edge changedEdge)
        {

            if (current.RepairVertices(changedEdge))
            {
                RefreshTwoPolygon(drawnPolygon, auxiliaryPolygon);
            }
            else
            {
                changedEdge.ClearStatus();
                foreach (Edge e in current.edges)
                {
                    if (current.RepairVertices(changedEdge)) break;
                }
                RefreshThreePolygon(old, drawnPolygon, auxiliaryPolygon);
            }

        }
        public void RepairAndRefreshBothPolygon(GKPolygon old, GKPolygon current, Vertice target)
        {
            int index = drawnPolygon.vertices.IndexOf(target);
            RepairAndRefreshBothPolygon(old, current, index);
        }
        public void RepairAndRefreshBothPolygon(GKPolygon old, GKPolygon current, int changedVerticeIndex)
        {
            if (current.RepairVertices(changedVerticeIndex))
            {
                RefreshTwoPolygon(drawnPolygon, auxiliaryPolygon);

            }
            else
            {
                drawnPolygon.vertices[changedVerticeIndex].ClearStatus();
                while (!current.RepairVertices((changedVerticeIndex)))
                {
                    changedVerticeIndex = (changedVerticeIndex + 1) % current.vertices.Count;
                }
                RefreshThreePolygon(old, drawnPolygon, auxiliaryPolygon);

            }
        }
        public void RepairAndRefreshPolygon(GKPolygon current, Edge changedEdge)
        {

            if (current.RepairVertices(changedEdge))
            {
                RefreshTwoPolygon(drawnPolygon, auxiliaryPolygon);
            }
            else
            {
                changedEdge.ClearStatus();
                foreach (Edge e in current.edges)
                {
                    if (current.RepairVertices(changedEdge)) break;
                }
                RefreshTwoPolygon(drawnPolygon, auxiliaryPolygon);
            }

        }
        public void RepairAndRefreshPolygon(GKPolygon current, Vertice target)
        {
            int index = drawnPolygon.vertices.IndexOf(target);
            RepairAndRefreshPolygon(current, index);
        }
        public void RepairAndRefreshPolygon(GKPolygon current, int changedVerticeIndex)
        {
            if (current.RepairVertices(changedVerticeIndex))
            {
                RefreshTwoPolygon(drawnPolygon, auxiliaryPolygon);
            }
            else
            {
                drawnPolygon.vertices[changedVerticeIndex].ClearStatus();
                while (!current.RepairVertices((changedVerticeIndex)))
                {
                    changedVerticeIndex = (changedVerticeIndex + 1) % current.vertices.Count;
                }
                RefreshTwoPolygon(drawnPolygon, auxiliaryPolygon);
            }
        }
        public void RefreshThreePolygon(GKPolygon old, GKPolygon current, GKPolygon auxiliary)
        {
            visuals.clear();

            visuals.redrawPolygon(auxiliary);
            visuals.redrawOldPolygon(old);
            visuals.redrawPolygon(current);

            UpdateScreen();
        }
        public void RefreshTwoPolygon(GKPolygon current, GKPolygon auxiliary)
        {
            visuals.clear();
            visuals.redrawPolygon(auxiliary);
            visuals.redrawPolygon(current);

            UpdateScreen();
        }

        //public void RefreshPolygon(GKPolygon polygon)
        //{
        //    visuals.redrawPolygon(polygon);
        //    UpdateScreen();
        //}


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
            oldPolygon = new GKPolygon(visuals);

            drawnPolygon = new GKPolygon(visuals);
            auxiliaryPolygon = new GKPolygon(visuals);
            RefreshTwoPolygon(drawnPolygon, auxiliaryPolygon);
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
            RefreshTwoPolygon(drawnPolygon, auxiliaryPolygon);
            dragWholePolygonCoords = pt;
        }

        private void drawingScreen_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragged)
            {
                oldPolygon = new GKPolygon(drawnPolygon);

                drawnPolygon.vertices[currentlyDragged].coords.X = Mouse.GetPosition(drawingScreen).X;
                drawnPolygon.vertices[currentlyDragged].coords.Y = Mouse.GetPosition(drawingScreen).Y;
                RepairAndRefreshBothPolygon(oldPolygon, drawnPolygon, currentlyDragged);
                return;
            }
            if (edgeDragged)
            {
                oldPolygon = new GKPolygon(drawnPolygon);

                currentlyedgeDragged.v1.coords.X = Vector.Add(-mouseToVertice1, Mouse.GetPosition(drawingScreen)).X;
                currentlyedgeDragged.v1.coords.Y = Vector.Add(-mouseToVertice1, Mouse.GetPosition(drawingScreen)).Y;
                currentlyedgeDragged.v2.coords.X = Vector.Add(-mouseToVertice2, Mouse.GetPosition(drawingScreen)).X;
                currentlyedgeDragged.v2.coords.Y = Vector.Add(-mouseToVertice2, Mouse.GetPosition(drawingScreen)).Y;
                RepairAndRefreshBothPolygon(oldPolygon, drawnPolygon, currentlyedgeDragged);

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
            else if (addPolygonMode) FinishAuxiliaryPolygon();
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

        private void FinishAuxiliaryPolygon()
        {
            if (auxiliaryPolygon.vertices.Count < 3)
            {
                MessageBox.Show("Polygon should have at least 3 vertices!", "Not enough vertices", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            auxiliaryPolygon.edges.Add(new Edge(auxiliaryPolygon.vertices.Last(), auxiliaryPolygon.vertices.First()));
            RefreshTwoPolygon(drawnPolygon, auxiliaryPolygon);
            addPolygonMode = false;
            endAddPolygonMode();
        }

        private void FinishPolygon()
        {
            if (drawnPolygon.vertices.Count < 3)
            {
                MessageBox.Show("Polygon should have at least 3 vertices!", "Not enough vertices", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            drawnPolygon.edges.Add(new Edge(drawnPolygon.vertices.Last(), drawnPolygon.vertices.First()));
            RefreshTwoPolygon(drawnPolygon, auxiliaryPolygon);
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
            addPolygonMode = true;
            addPolygon.IsEnabled = false;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            visuals.algorithm = Algorithm.WU;
            RefreshThreePolygon(oldPolygon, drawnPolygon, auxiliaryPolygon);
            aliasing.IsChecked = false;
        }

        private void aliasing_Checked(object sender, RoutedEventArgs e)
        {
            visuals.algorithm = Algorithm.Bresenham;
            RefreshTwoPolygon(drawnPolygon, auxiliaryPolygon);
            antiAliasing.IsChecked = false;

        }
    }





}
