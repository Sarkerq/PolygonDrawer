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

namespace GK1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GKPolygon drawnPolygon = new GKPolygon();
        private GKPolygon oldPolygon = new GKPolygon();
        const double verticeRadius = 9;
        const int lineWidth = 3;
        bool dragged = false;
        int currentlyDragged = 0;
        bool newPolygonMode = true;
        private int iter;

        public MainWindow()
        {
            InitializeComponent();
            //System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            //dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            //dispatcherTimer.Interval = new TimeSpan(100000);
            //dispatcherTimer.Start();


        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {

        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (dragged)
            {
                drawnPolygon.vertices[currentlyDragged].xValue = e.GetPosition(drawingScreen).X;
                drawnPolygon.vertices[currentlyDragged].yValue = e.GetPosition(drawingScreen).Y;
                repairAndRedrawPolygon(drawnPolygon);
                dragged = false;
                return;
            }
            Vertice targetVertice = clickedVertice(e.GetPosition(drawingScreen));
            if (targetVertice != null) onClickedVertice(targetVertice);
            else if (newPolygonMode) onNewVertice(e.GetPosition(drawingScreen));

        }

        private void onNewVertice(Point location)
        {
            Vertice newV = new Vertice(location);
            placeVerticeOnCanvas(newV);
            if (newPolygonMode && drawnPolygon.vertices.Count >= 1) drawConnection(drawnPolygon.vertices.Last(), newV);

            drawnPolygon.vertices.Add(newV);
            //repairAndRedrawPolygon(drawnPolygon);
        }

        private void onClickedVertice(Vertice target)
        {
            if (newPolygonMode)
            {
                if (target == drawnPolygon.vertices.First())
                {
                    drawConnection(drawnPolygon.vertices.Last(), drawnPolygon.vertices.First());
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
                    placeColorfulVerticeOnCanvas(v, Color.FromArgb(255, 255, 0, 0));
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

        void repairAndRedrawPolygon(GKPolygon polygon)
        {
            polygon.repairVertices();
            if (drawnPolygon.vertices.Count >= 2)
            {
                drawingScreen.Children.Clear();
                Vertice first = drawnPolygon.vertices[0];
                placeVerticeOnCanvas(first);
                for (int i = 1; i < drawnPolygon.vertices.Count; i++)
                {
                    placeVerticeOnCanvas(drawnPolygon.vertices[i]);
                    drawConnection(drawnPolygon.vertices[i - 1], drawnPolygon.vertices[i]);
                }
                drawConnection(drawnPolygon.vertices.Last(), first);
            }
        }

        private void drawConnection(Vertice v1, Vertice v2)
        {
            line((int)v1.xValue, (int)v1.yValue, (int)v2.xValue, (int)v2.yValue, 1);

        }
        public void line(int x, int y, int x2, int y2, int color)
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
                putpixel(x, y, color);
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

        private void putpixel(int x, int y, int color)
        {
            Rectangle rec = new Rectangle();
            Canvas.SetTop(rec, y - lineWidth / 2);
            Canvas.SetLeft(rec, x - lineWidth / 2);
            rec.Width = lineWidth;
            rec.Height = lineWidth;
            rec.Fill = new SolidColorBrush(Colors.Red);
            drawingScreen.Children.Add(rec);
        }

        void drawVertice(Vertice v)
        {
            throw new NotImplementedException();
        }

        private void newPolygon_Click(object sender, RoutedEventArgs e)
        {
            drawnPolygon = new GKPolygon();
            drawingScreen.Children.Clear();
            newPolygonMode = true;
            dragged = false;
        }

        private void drawingScreen_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void drawingScreen_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragged)
            {
                drawnPolygon.vertices[currentlyDragged].xValue = Mouse.GetPosition(drawingScreen).X;
                drawnPolygon.vertices[currentlyDragged].yValue = Mouse.GetPosition(drawingScreen).Y;
                repairAndRedrawPolygon(drawnPolygon);
            }

        }
    }
    enum VerticeState
    {
        Clockwise,
        CounterClockwise,
        None
    }
    class Vertice
    {
        public VerticeState fixedVertical;
        public VerticeState fixedHorizontal;
        public bool fixedAngle;
        public int fixedAngleValue;
        public double xValue;
        public double yValue;
        public Vertice(double x, double y)
        {
            fixedVertical = fixedHorizontal = VerticeState.None;
            fixedAngle = false;
            xValue = x;
            yValue = y;
        }
        public Vertice(Point p)
        {
            fixedVertical = fixedHorizontal = VerticeState.None;
            fixedAngle = false;
            xValue = p.X;
            yValue = p.Y;
        }
        public Vertice(Vertice v)
        {
            fixedVertical = v.fixedVertical;
            fixedHorizontal = v.fixedHorizontal;
            fixedAngle = v.fixedAngle;
            fixedAngleValue = v.fixedAngleValue;
            xValue = v.xValue;
            yValue = v.yValue;
        }
    }
    class GKPolygon
    {
        public List<Vertice> vertices = new List<Vertice>();
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
