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
        Carbon visuals = new Carbon();
        DispatcherTimer timer;

        public GKPolygon drawnPolygon = new GKPolygon();
        private GKPolygon oldPolygon = new GKPolygon();


        internal void putVerticeInTheMiddle(Edge target)
        {
            int indexVert = 0;
            int indexEdge = 0;
            clearStatus(target.v1);
            clearStatus(target.v2);

            for (int i = 0; i < drawnPolygon.vertices.Count; i++)
            {
                if (target.v1 == drawnPolygon.vertices[i] || target.v2 == drawnPolygon.vertices[i]) indexVert = i;
                if (target == drawnPolygon.edges[i]) indexEdge = i;
            }
            Point newVerticeCoords = new Point((target.v1.coords.X + target.v2.coords.X) / 2,
                                                (target.v1.coords.Y + target.v2.coords.Y) / 2);
            Vertice newV = new Vertice(newVerticeCoords);
            drawnPolygon.vertices.Insert(indexVert, newV);
            drawnPolygon.edges.RemoveAt(indexEdge);
            drawnPolygon.edges.Insert(indexEdge, new Edge(target.v1, newV));
            drawnPolygon.edges.Insert(indexEdge + 1, new Edge(newV, target.v2));
            redrawPolygon(drawnPolygon);
        }

        internal void repairAndRedrawPolygon(GKPolygon drawnPolygon, Vertice target)
        {
            int index = drawnPolygon.vertices.IndexOf(target);
            repairAndRedrawPolygon(drawnPolygon, index);
        }

        private void clearStatus(Vertice v1)
        {
            v1.fixedAngle = false;
            v1.fixedAngleValue = 0;
            v1.fixedHorizontal = VerticeState.None;
            v1.fixedVertical = VerticeState.None;
        }

        public void forceAngle(Vertice target, double angle, VerticeFix fix)
        {
            Vertice left, right;
            Edge[] pair = new Edge[2];
            int i = 0;
            target.fixedAngle = true;
            target.fixedAngleValue = angle;
            foreach (Edge e in drawnPolygon.edges)
            {
                if (e.v1 == target || e.v2 == target)
                {
                    pair[i] = e;
                    i++;
                }
            }
            if (pair[0].v1 == target) left = pair[0].v2;
            else left = pair[0].v1;
            if (pair[1].v1 == target) right = pair[1].v2;
            else right = pair[1].v1;

            //double originalAngle = getAngle(left, target, right);
            //if(originalAngle < 0)
            //{
            //    Vertice tmp = left;
            //    left = right;
            //    right = tmp;
            //}

            double originalLeftAngle = angleAgainstXAxis(target.coords, left.coords);
            if (fix == VerticeFix.Left)
            {
                Polar pol = new Polar(target.coords, distance(target.coords, right.coords), angle + originalLeftAngle);
                Point result = pol.toCartesian();
                right.coords = result;
            }
            else
            {
                Polar pol = new Polar(target.coords, distance(target.coords, left.coords), angle + originalLeftAngle);
                Point result = pol.toCartesian();
                left.coords = result;
            }
        }

        private double getAngle(Vertice left, Vertice target, Vertice right)
        {
            return angleAgainstXAxis(target.coords, left.coords) - angleAgainstXAxis(target.coords, right.coords);
        }

        bool dragged = false;

        internal void forceVertical(Edge target)
        {

            Point bottom = target.v1.coords;
            Point top = target.v2.coords;
            if (bottom.Y > top.Y)
            {
                Point tmp = bottom;
                bottom = top;
                top = tmp;

                Polar pol = new Polar(bottom, distance(bottom, top), Math.PI / 2);
                target.v1.coords = pol.toCartesian();

                target.v1.fixedVertical = VerticeState.Top;
                target.v2.fixedVertical = VerticeState.Bottom;
            }
            else
            {
                Polar pol = new Polar(bottom, distance(bottom, top), Math.PI / 2);
                target.v2.coords = pol.toCartesian();

                target.v1.fixedVertical = VerticeState.Bottom;
                target.v2.fixedVertical = VerticeState.Top;

            }
            target.state = EdgeState.Vertical;

        }

        bool edgeDragged = false;
        int currentlyDragged = 0;

        internal void forceHorizontal(Edge target)
        {
            Point left = target.v1.coords;
            Point right = target.v2.coords;
            if (left.X > right.X)
            {
                Point tmp = left;
                left = right;
                right = tmp;

                Polar pol = new Polar(left, distance(left, right), 0);
                target.v1.coords = pol.toCartesian();
                target.v1.fixedHorizontal = VerticeState.Right;
                target.v2.fixedHorizontal = VerticeState.Left;

            }
            else
            {
                Polar pol = new Polar(left, distance(left, right), 0);
                target.v2.coords = pol.toCartesian();
                target.v1.fixedHorizontal = VerticeState.Left;
                target.v2.fixedHorizontal = VerticeState.Right;
            }
            target.state = EdgeState.Horizontal;

        }

        Edge currentlyedgeDragged;
        Vector mouseToVertice1;
        Vector mouseToVertice2;
        bool newPolygonMode = true;
        private Point dragWholePolygonCoords;
        private bool polygonDragged;
        private double epsilon;

        public MainWindow()
        {
            InitializeComponent();



            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += UpdateScreen;
            timer.Start();

        }
        void UpdateScreen(object o, EventArgs e)
        {
            bitmap = BitmapSource.Create(visuals.width, visuals.height,
                96, 96, visuals.pf, null, visuals.pixelData, visuals.rawStride);
            lineCarbon.Source = bitmap;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            Vertice targetVertice = clickedVertice(e.GetPosition(drawingScreen));
            if (targetVertice != null) onClickedVertice(targetVertice);
            else
            {
                Edge targetEdge = clickedEdge(e.GetPosition(drawingScreen));
                if (targetEdge != null) onClickedEdge(targetEdge, e);
                else if (newPolygonMode) onNewVertice(e.GetPosition(drawingScreen));
                else
                {
                    onMovedPolygon(e);
                }

            }
        }

        private void onMovedPolygon(MouseButtonEventArgs e)
        {
            dragWholePolygonCoords = e.GetPosition(drawingScreen);
            polygonDragged = true;
        }

        private void onClickedEdge(Edge targetEdge, MouseButtonEventArgs e)
        {
            mouseToVertice1 = new Vector(e.GetPosition(drawingScreen).X - targetEdge.v1.coords.X,
                                         e.GetPosition(drawingScreen).Y - targetEdge.v1.coords.Y);
            mouseToVertice2 = new Vector(e.GetPosition(drawingScreen).X - targetEdge.v2.coords.X,
                                         e.GetPosition(drawingScreen).Y - targetEdge.v2.coords.Y);
            edgeDragged = true;
            currentlyedgeDragged = targetEdge;
        }

        private Edge clickedEdge(Point point)
        {
            int xIndex = (int)point.X * 3;
            int yIndex = (int)point.Y * visuals.rawStride;
            return visuals.pixelOwner[xIndex + yIndex];
        }

        private void onNewVertice(Point location)
        {
            Vertice newV = new Vertice(location);
            if (newPolygonMode && drawnPolygon.vertices.Count >= 1)
            {
                drawnPolygon.edges.Add(new Edge(drawnPolygon.vertices.Last(), newV));
            }
            drawnPolygon.vertices.Add(newV);
            redrawPolygon(drawnPolygon);
        }

        private void onClickedVertice(Vertice target)
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
            redrawPolygon(drawnPolygon);
        }

        private Vertice clickedVertice(Point location)
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
 



        public void repairAndRedrawPolygon(GKPolygon polygon, int changedVerticeIndex)
        {

            repairVertices(drawnPolygon, changedVerticeIndex);
            redrawPolygon(drawnPolygon);
        }

        private bool repairVertices(GKPolygon polygon, int changedVerticeIndex)
        {
            return true;
            bool[] verticeFixed = new bool[polygon.vertices.Count];
            int iter = changedVerticeIndex;
            bool first = true;
            // vertices following
            while (first || iter != changedVerticeIndex)
            {
                first = false;
                Vertice v = polygon.vertices[iter];
                if (isCorrectVertice(v, true)) break;
                else repairVertice(v, true);


                iter = (iter + 1) % polygon.vertices.Count;

            }
            if (iter == changedVerticeIndex) return false;
            int newIter = changedVerticeIndex;
            first = true;
            while (first || newIter != iter)
            {
                first = false;
                Vertice v = polygon.vertices[iter];
                if (isCorrectVertice(v, false)) break;
                else repairVertice(v, false);


                newIter = (newIter - 1 + polygon.vertices.Count) % polygon.vertices.Count;

            }
            if (iter == newIter) return false;
            return true;
        }

        private void repairVertice(Vertice v, bool front)
        {

            int edgeIndex;
            if (front)
                edgeIndex = drawnPolygon.vertices.IndexOf(v);
            else
                edgeIndex = (drawnPolygon.vertices.IndexOf(v) - 1 + drawnPolygon.vertices.Count) % drawnPolygon.vertices.Count;

            Edge chosen = drawnPolygon.edges[edgeIndex];
            Vertice toChange;
            if (chosen.v1 == v)
                toChange = chosen.v2;
            else
                toChange = chosen.v1;
            if(v.fixedAngle)
            {
                forceAngle(v, v.fixedAngleValue, chosen);
            }
            if(chosen.state == EdgeState.Horizontal)
            {

            }
            if (chosen.state == EdgeState.Vertical)
            {

            }

        }

        private void forceAngle(Vertice target, double angle, Edge chosen)
        {
            Vertice left, right;
            Edge[] pair = new Edge[2];
            int i = 0;
            target.fixedAngle = true;
            target.fixedAngleValue = angle;
            foreach (Edge e in drawnPolygon.edges)
            {
                if (e.v1 == target || e.v2 == target)
                {
                    pair[i] = e;
                    i++;
                }
            }
            if (pair[0].v1 == target) left = pair[0].v2;
            else left = pair[0].v1;
            if (pair[1].v1 == target) right = pair[1].v2;
            else right = pair[1].v1;

            //double originalAngle = getAngle(left, target, right);
            //if(originalAngle < 0)
            //{
            //    Vertice tmp = left;
            //    left = right;
            //    right = tmp;
            //}

            double originalLeftAngle = angleAgainstXAxis(target.coords, left.coords);
            if (chosen == pair[0])
            {
                Polar pol = new Polar(target.coords, distance(target.coords, right.coords), angle + originalLeftAngle);
                Point result = pol.toCartesian();
                right.coords = result;
            }
            else
            {
                Polar pol = new Polar(target.coords, distance(target.coords, left.coords), angle + originalLeftAngle);
                Point result = pol.toCartesian();
                left.coords = result;
            }
        }

        public void repairAndRedrawPolygon(GKPolygon polygon, Edge changedEdge)
        {

            repairVertices(drawnPolygon, changedEdge);
            redrawPolygon(drawnPolygon);
        }

        private void repairVertices(GKPolygon drawnPolygon, Edge changedEdge)
        {
            repairVertices(drawnPolygon, drawnPolygon.vertices.IndexOf(changedEdge.v1));
        }

        public void redrawPolygon(GKPolygon drawnPolygon)
        {
            if (drawnPolygon.vertices.Count >= 1)
            {
                clearCarbon();

                Vertice first = drawnPolygon.vertices[0];


                for (int i = 0; i < drawnPolygon.edges.Count; i++)
                {
                    visuals.drawEdge(drawnPolygon.edges[i], Colors.Red);
                }
                for (int i = 0; i < drawnPolygon.vertices.Count; i++)
                    visuals.drawVertice(drawnPolygon.vertices[i]);


                UpdateScreen(null, null);
            }
        }

        private void clearCanvas()
        {
            drawingScreen.Children.Clear();
            drawingScreen.Children.Add(lineCarbon);
        }


        private void newPolygon_Click(object sender, RoutedEventArgs e)
        {
            drawnPolygon = new GKPolygon();
            clearCanvas();
            clearCarbon();
            newPolygonMode = true;
            dragged = false;
        }

        private void clearCarbon()
        {
            visuals.pixelData = new byte[visuals.rawStride * visuals.height];
            //for (int i = 0; i < visuals.rawStride * visuals.height; i++)
            //    visuals.pixelData[i] = 255;
            visuals.pixelOwner = new Edge[visuals.rawStride * visuals.height];
        }

        private void drawingScreen_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (dragged)
            {
                drawnPolygon.vertices[currentlyDragged].coords.X = e.GetPosition(drawingScreen).X;
                drawnPolygon.vertices[currentlyDragged].coords.Y = e.GetPosition(drawingScreen).Y;
                repairAndRedrawPolygon(drawnPolygon,currentlyDragged);
                dragged = false;
                return;
            }
            if (edgeDragged)
            {
                currentlyedgeDragged.v1.coords.X = Vector.Add(-mouseToVertice1, Mouse.GetPosition(drawingScreen)).X;
                currentlyedgeDragged.v1.coords.Y = Vector.Add(-mouseToVertice1, Mouse.GetPosition(drawingScreen)).Y;
                currentlyedgeDragged.v2.coords.X = Vector.Add(-mouseToVertice2, Mouse.GetPosition(drawingScreen)).X;
                currentlyedgeDragged.v2.coords.Y = Vector.Add(-mouseToVertice2, Mouse.GetPosition(drawingScreen)).Y;
                repairAndRedrawPolygon(drawnPolygon,currentlyedgeDragged);
                edgeDragged = false;
                return;
            }
            if (polygonDragged)
            {
                draggingPolygon(e.GetPosition(drawingScreen));
                polygonDragged = false;
                return;
            }
        }

        private void draggingPolygon(Point pt)
        {
            Vector displacement = pt - dragWholePolygonCoords;
            foreach (Vertice v in drawnPolygon.vertices)
            {
                v.coords += displacement;
            }
            redrawPolygon(drawnPolygon);
            dragWholePolygonCoords = pt;
        }

        private void drawingScreen_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragged)
            {
                drawnPolygon.vertices[currentlyDragged].coords.X = Mouse.GetPosition(drawingScreen).X;
                drawnPolygon.vertices[currentlyDragged].coords.Y = Mouse.GetPosition(drawingScreen).Y;
                repairAndRedrawPolygon(drawnPolygon, currentlyDragged);
                return;
            }
            if (edgeDragged)
            {
                currentlyedgeDragged.v1.coords.X = Vector.Add(-mouseToVertice1, Mouse.GetPosition(drawingScreen)).X;
                currentlyedgeDragged.v1.coords.Y = Vector.Add(-mouseToVertice1, Mouse.GetPosition(drawingScreen)).Y;
                currentlyedgeDragged.v2.coords.X = Vector.Add(-mouseToVertice2, Mouse.GetPosition(drawingScreen)).X;
                currentlyedgeDragged.v2.coords.Y = Vector.Add(-mouseToVertice2, Mouse.GetPosition(drawingScreen)).Y;
                repairAndRedrawPolygon(drawnPolygon, currentlyedgeDragged);

                return;
            }
            if (polygonDragged)
            {
                draggingPolygon(e.GetPosition(drawingScreen));
                return;
            }

        }
        double distance(Vertice v1, Vertice v2)
        {
            return Math.Sqrt(Math.Pow(v1.coords.X - v2.coords.X, 2) + Math.Pow(v1.coords.Y - v2.coords.Y, 2));
        }
        double distance(Point x, Point y)
        {
            return Math.Sqrt(Math.Pow(x.X - y.X, 2) + Math.Pow(x.Y - y.Y, 2));
        }
        Polar cartesianToPolar(Point x, Point y)
        {
            return new Polar(x, distance(x, y), angleAgainstXAxis(x, y));
        }

        private double angleAgainstXAxis(Point x, Point y)
        {
            Vector yRelative = y - x;
            return Math.Atan2(yRelative.Y, yRelative.X);
        }

        private void drawingScreen_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Vertice targetVertice = clickedVertice(e.GetPosition(drawingScreen));
            if (targetVertice != null) onRightClickedVertice(targetVertice);
            else
            {
                Edge targetEdge = clickedEdge(e.GetPosition(drawingScreen));
                if (targetEdge != null) onRightClickedEdge(targetEdge, e);
                else if (newPolygonMode) finishPolygon();
            }

        }

        private void finishPolygon()
        {
            if (drawnPolygon.vertices.Count < 3)
            {
                MessageBox.Show("Polygon should have at least 3 vertices!", "Not enough vertices", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            drawnPolygon.edges.Add(new Edge(drawnPolygon.vertices.Last(), drawnPolygon.vertices.First()));
            redrawPolygon(drawnPolygon);
            newPolygonMode = false;
            //throw new NotImplementedException();
        }

        private void onRightClickedEdge(Edge targetEdge, MouseButtonEventArgs e)
        {
            showModifyEdgeWindow(targetEdge);
        }

        private void showModifyEdgeWindow(Edge modificationTarget)
        {
            EdgeSettings window = new EdgeSettings(this, modificationTarget);
            window.Show();
        }

        private void onRightClickedVertice(Vertice targetVertice)
        {
            showSetAngleWindow(targetVertice);
        }

        private void showSetAngleWindow(Vertice modificationTarget)
        {
            SetAngle window = new SetAngle(this, modificationTarget);
            window.Show();
        }

        internal bool isCorrectVertice(Vertice v, bool front)
        {
            int edgeIndex;
            if (front)
                edgeIndex = drawnPolygon.vertices.IndexOf(v);
            else
                edgeIndex = (drawnPolygon.vertices.IndexOf(v) - 1 + drawnPolygon.vertices.Count) % drawnPolygon.vertices.Count;
            if (!v.fixedAngle && drawnPolygon.edges[edgeIndex].state == EdgeState.None) return true;
            if (v.fixedAngle && correctAngle(v)) return true;
            if (drawnPolygon.edges[edgeIndex].state == EdgeState.Horizontal && correctHorizontal(drawnPolygon.edges[edgeIndex])) return true;
            if (drawnPolygon.edges[edgeIndex].state == EdgeState.Vertical && correctVertical(drawnPolygon.edges[edgeIndex])) return true;

            return false;
        }

        private bool correctVertical(Edge e)
        {
            epsilon = 0.5;
            return Math.Abs(e.v1.coords.X - e.v2.coords.X) < epsilon;
        }

        private bool correctHorizontal(Edge e)
        {
            epsilon = 0.5;
            return Math.Abs(e.v1.coords.Y - e.v2.coords.Y) < epsilon;

        }

        private bool correctAngle(Vertice v)
        {
            int index = drawnPolygon.vertices.IndexOf(v);
            epsilon = 0.5 * Math.PI / 180;
            return Math.Abs(getAngle(
                             drawnPolygon.vertices[(index - 1 + drawnPolygon.vertices.Count) % drawnPolygon.vertices.Count],
                             v,
                             drawnPolygon.vertices[(index + 1) % drawnPolygon.vertices.Count])
                             - v.fixedAngleValue) < epsilon;

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
    public class Edge
    {
        public Vertice v1, v2;
        public EdgeState state;
        public Edge(Vertice _v1, Vertice _v2)
        {
            v1 = _v1;
            v2 = _v2;
            state = EdgeState.None;
        }
    }
    public class GKPolygon
    {
        public List<Vertice> vertices = new List<Vertice>();
        public List<Edge> edges = new List<Edge>();


    }

}
