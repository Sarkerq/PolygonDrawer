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
    public partial class MainWindow : Window
    {
        BitmapSource bitmap;
        Carbon visuals = new Carbon();
        DispatcherTimer timer;

        private GKPolygon drawnPolygon = new GKPolygon();
        private GKPolygon oldPolygon = new GKPolygon();
        const double verticeRadius = 9;
        const int lineWidth = 3;
        bool dragged = false;
        bool draggedEdge = false;
        int currentlyDragged = 0;
        Edge currentlyDraggedEdge;
        Vector mouseToVertice1;
        Vector mouseToVertice2;
        bool newPolygonMode = true;
        private int iter;

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
            }
        }

        private void onClickedEdge(Edge targetEdge, MouseButtonEventArgs e)
        {
            mouseToVertice1 = new Vector(e.GetPosition(drawingScreen).X - targetEdge.v1.xValue,
                                         e.GetPosition(drawingScreen).Y - targetEdge.v1.yValue);
            mouseToVertice2 = new Vector(e.GetPosition(drawingScreen).X - targetEdge.v2.xValue,
                                         e.GetPosition(drawingScreen).Y - targetEdge.v2.yValue);
            drawEdge(targetEdge, Colors.Yellow);
            draggedEdge = true;
            currentlyDraggedEdge = targetEdge;
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
            placeVerticeOnCarbon(newV);
            if (newPolygonMode && drawnPolygon.vertices.Count >= 1)
            {
                drawnPolygon.edges.Add(new Edge(drawnPolygon.vertices.Last(), newV));

                drawEdge(drawnPolygon.edges.Last(), Colors.Red);
            }
            drawnPolygon.vertices.Add(newV);
            //repairAndRedrawPolygon(drawnPolygon);
        }

        private void onClickedVertice(Vertice target)
        {
            if (newPolygonMode)
            {
                if (target == drawnPolygon.vertices.First())
                {
                    drawnPolygon.edges.Add(new Edge(drawnPolygon.vertices.Last(), drawnPolygon.vertices.First()));
                    drawEdge(drawnPolygon.edges.Last(), Colors.Red);
                    newPolygonMode = false;
                }

            }
            else
            {
                dragged = true;
                currentlyDragged = drawnPolygon.vertices.IndexOf(target);
            }
            repairAndRedrawPolygon(drawnPolygon);
        }

        private Vertice clickedVertice(Point location)
        {
            foreach (Vertice v in drawnPolygon.vertices)
            {
                if (Math.Abs(v.xValue - location.X) <= verticeRadius && Math.Abs(v.yValue - location.Y) <= verticeRadius)
                {
                    //placeColorfulVerticeOnCanvas(v, Color.FromArgb(255, 255, 0, 0));
                    return v;
                }
            }
            return null;
        }
        void placeColorfulVerticeOnCanvas(Vertice v, Color c)
        {
            // Create a red Ellipse.
            Ellipse myEllipse = new Ellipse();

            // Create a SolidColorBrush with a red color to fill the 
            // Ellipse with.
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();

            // Describes the brush's color using RGB values. 
            // Each value has a range of 0-255.
            mySolidColorBrush.Color = c;
            myEllipse.Fill = mySolidColorBrush;
            myEllipse.StrokeThickness = 2;
            myEllipse.Stroke = Brushes.Black;

            // Set the width and height of the Ellipse.
            myEllipse.Width = 2 * verticeRadius;
            myEllipse.Height = 2 * verticeRadius;
            // Add the Ellipse to the StackPanel.
            drawingScreen.Children.Add(myEllipse);
            Canvas.SetTop(myEllipse, v.yValue - verticeRadius);
            Canvas.SetLeft(myEllipse, v.xValue - verticeRadius);
        }
        void placeVerticeOnCanvas(Vertice v)
        {
            // Create a red Ellipse.
            Ellipse myEllipse = new Ellipse();

            // Create a SolidColorBrush with a red color to fill the 
            // Ellipse with.
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();

            // Describes the brush's color using RGB values. 
            // Each value has a range of 0-255.
            mySolidColorBrush.Color = Color.FromArgb(255, 255, 255, 0);
            myEllipse.Fill = mySolidColorBrush;
            myEllipse.StrokeThickness = 2;
            myEllipse.Stroke = Brushes.Black;

            // Set the width and height of the Ellipse.
            myEllipse.Width = 2 * verticeRadius;
            myEllipse.Height = 2 * verticeRadius;
            // Add the Ellipse to the StackPanel.
            drawingScreen.Children.Add(myEllipse);
            Canvas.SetTop(myEllipse, v.yValue - verticeRadius);
            Canvas.SetLeft(myEllipse, v.xValue - verticeRadius);
        }

        void placeVerticeOnCarbon(Vertice v)
        {
            for (int y = (int)v.yValue - (int)verticeRadius; y < (int)v.yValue + (int)verticeRadius; y++)
                for (int x = (int)v.xValue - (int)verticeRadius; x < (int)v.xValue + (int)verticeRadius; x++)

                    SetPixel(x, y, Colors.Green, visuals);
        }

        void repairAndRedrawPolygon(GKPolygon polygon)
        {

            polygon.repairVertices();
            if (drawnPolygon.vertices.Count >= 2)
            {
                //clearCanvas();
                clearCarbon();

                Vertice first = drawnPolygon.vertices[0];


                for (int i = 0; i < drawnPolygon.edges.Count; i++)
                {
                    drawEdge(drawnPolygon.edges[i], Colors.Red);
                }
                for (int i = 0; i < drawnPolygon.vertices.Count; i++)
                    placeVerticeOnCarbon(drawnPolygon.vertices[i]);


                UpdateScreen(null, null);
            }
        }

        private void clearCanvas()
        {
            drawingScreen.Children.Clear();
            drawingScreen.Children.Add(lineCarbon);
        }

        private void drawEdge(Edge con, Color c)
        {
            line(con, (int)con.v1.xValue, (int)con.v1.yValue, (int)con.v2.xValue, (int)con.v2.yValue, c);

        }
        public void line(Edge con, int x, int y, int x2, int y2, Color color)
        {
            int w = x2 - x;
            int h = y2 - y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = longest >> 1;
            for (int i = 0; i <= longest; i++)
            {
                for (int j = -1; j <= 1; j++)
                    for (int k = -1; k <= 1; k++)
                        SetPixel(con, x + k, y + j, color, visuals);
                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    x += dx1;
                    y += dy1;
                }
                else
                {
                    x += dx2;
                    y += dy2;
                }
            }
        }

        //private void putpixel(int x, int y, int color)
        //{
        //    Rectangle rec = new Rectangle();
        //    Canvas.SetTop(rec, y - lineWidth / 2);
        //    Canvas.SetLeft(rec, x - lineWidth / 2);
        //    rec.Width = lineWidth;
        //    rec.Height = lineWidth;
        //    rec.Fill = new SolidColorBrush(Colors.Red);
        //    drawingScreen.Children.Add(rec);
        //}
        void SetPixel(Edge owner, int x, int y, Color c, Carbon visuals)
        {
            int xIndex = x * 3;
            int yIndex = y * visuals.rawStride;
            visuals.pixelData[xIndex + yIndex] = c.R;
            visuals.pixelData[xIndex + yIndex + 1] = c.G;
            visuals.pixelData[xIndex + yIndex + 2] = c.B;
            visuals.pixelOwner[xIndex + yIndex] = owner;
        }
        void SetPixel(int x, int y, Color c, Carbon visuals)
        {
            int xIndex = x * 3;
            int yIndex = y * visuals.rawStride;
            visuals.pixelData[xIndex + yIndex] = c.R;
            visuals.pixelData[xIndex + yIndex + 1] = c.G;
            visuals.pixelData[xIndex + yIndex + 2] = c.B;
        }
        void drawVertice(Vertice v)
        {
            throw new NotImplementedException();
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
            visuals.pixelOwner = new Edge[visuals.rawStride * visuals.height];
        }

        private void drawingScreen_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (dragged)
            {
                drawnPolygon.vertices[currentlyDragged].xValue = e.GetPosition(drawingScreen).X;
                drawnPolygon.vertices[currentlyDragged].yValue = e.GetPosition(drawingScreen).Y;
                repairAndRedrawPolygon(drawnPolygon);
                dragged = false;
                return;
            }
            if (draggedEdge)
            {
                currentlyDraggedEdge.v1.xValue = Vector.Add(-mouseToVertice1, Mouse.GetPosition(drawingScreen)).X;
                currentlyDraggedEdge.v1.yValue = Vector.Add(-mouseToVertice1, Mouse.GetPosition(drawingScreen)).Y;
                currentlyDraggedEdge.v2.xValue = Vector.Add(-mouseToVertice2, Mouse.GetPosition(drawingScreen)).X;
                currentlyDraggedEdge.v2.yValue = Vector.Add(-mouseToVertice2, Mouse.GetPosition(drawingScreen)).Y;
                repairAndRedrawPolygon(drawnPolygon);
                draggedEdge = false;
                return;
            }
        }

        private void drawingScreen_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragged)
            {
                drawnPolygon.vertices[currentlyDragged].xValue = Mouse.GetPosition(drawingScreen).X;
                drawnPolygon.vertices[currentlyDragged].yValue = Mouse.GetPosition(drawingScreen).Y;
                repairAndRedrawPolygon(drawnPolygon);
                return;
            }
            if (draggedEdge)
            {
                currentlyDraggedEdge.v1.xValue = Vector.Add(-mouseToVertice1, Mouse.GetPosition(drawingScreen)).X;
                currentlyDraggedEdge.v1.yValue = Vector.Add(-mouseToVertice1, Mouse.GetPosition(drawingScreen)).Y;
                currentlyDraggedEdge.v2.xValue = Vector.Add(-mouseToVertice2, Mouse.GetPosition(drawingScreen)).X;
                currentlyDraggedEdge.v2.yValue = Vector.Add(-mouseToVertice2, Mouse.GetPosition(drawingScreen)).Y;
                repairAndRedrawPolygon(drawnPolygon);

                return;
            }

        }
        double distance(Vertice v1, Vertice v2)
        {
            return Math.Sqrt(Math.Pow(v1.xValue - v2.xValue, 2) + Math.Pow(v1.yValue - v2.yValue, 2));
        }
        double distance(Point x, Point y)
        {
            return Math.Sqrt(Math.Pow(x.X - y.X, 2) + Math.Pow(x.Y - y.Y, 2));
        }
        Polar cartesianToPolar(Point x, Point y)
        {
            return new Polar(x, distance(x, y), 0);
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
            //throw new NotImplementedException();
        }

        private void onRightClickedEdge(Edge targetEdge, MouseButtonEventArgs e)
        {
            showModifyEdgeWindow(targetEdge);
        }

        private void showModifyEdgeWindow(Edge targetEdge)
        {
            throw new NotImplementedException();
        }

        private void onRightClickedVertice(Vertice targetVertice)
        {
            showSetAngleWindow(targetVertice);
        }

        private void showSetAngleWindow(Vertice targetVertice)
        {
            throw new NotImplementedException();
        }
    }
    public class Edge
    {
        public Vertice v1, v2;
        public Edge(Vertice _v1, Vertice _v2)
        {
            v1 = _v1;
            v2 = _v2;
        }
    }
    public class GKPolygon
    {
        public List<Vertice> vertices = new List<Vertice>();
        public List<Edge> edges = new List<Edge>();

        public void repairVertices()
        {
            return;
            bool[] verticeRepaired = new bool[vertices.Count];
            bool polygonRepaired = false;
            while (!polygonRepaired)
            {








                polygonRepaired = true;
                foreach (bool repaired in verticeRepaired)
                {
                    if (!repaired)
                    {
                        polygonRepaired = false;
                        break;
                    }
                }
            }
        }
    }

}
