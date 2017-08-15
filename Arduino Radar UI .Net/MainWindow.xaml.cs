using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.IO.Ports;

namespace Arduino_Radar_UI.Net
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region General Declaration

        private delegate void AddGraphics();
        private delegate void connectionParser(string data);
        SerialPort arduinoPort = new SerialPort();
        public static double lineLength;
        private Thread trdBGRunner;
        Thread updateIO;
        private int radarAngle;
        private double distance = 0;
        bool turnLeft = true, ArduinoMode = false;

        List<Label> lblDistanceList = new List<Label>(), lblAngleList = new List<Label>();
        List<Line> staticLineList = new List<Line>();
        List<System.Windows.Shapes.Path> staticPathList = new List<System.Windows.Shapes.Path>();
        List<Line> radarRadius = new List<Line>();
        List<int> angleList = new List<int>();

        #endregion

        #region Auto Generated Code

        public MainWindow()
        {
            InitializeComponent();
            radarAngle = 0;
            //this.Icon =(System.Drawing.Icon)Properties.Resources.icon;
        }

        #endregion

        #region Thread's Functions/Procedures

        public void Run()
        {
            trdBGRunner = new Thread(new ThreadStart(CursorRunner));
            trdBGRunner.IsBackground = true;
            trdBGRunner.Start();
            this.Dispatcher.Invoke(new AddGraphics(radarCursorAngleChange));
            updateIO = new Thread(new ThreadStart(UpdateSerial));
            updateIO.IsBackground = true;
            updateIO.Start();
        }

        private void UpdateSerial()
        {
            string received = string.Empty;
            string strAngle = string.Empty, strDistance = string.Empty;
            while (true)
            {
                try
                {
                    if (arduinoPort.IsOpen)
                    {
                        received = arduinoPort.ReadLine();
                        if (received.Contains(',') && received.Contains('.'))
                        {
                            txtSerialInput.Dispatcher.Invoke(new connectionParser(display_input), received);
                            arduinoPort.DiscardOutBuffer();
                            strAngle = received.Split(',').First();
                            radarAngle = Convert.ToInt32(strAngle);
                            strDistance = received.Split('.').First().Split(',').Last();
                            distance = 0.01 * Convert.ToDouble(strDistance);
                            received = string.Empty;
                            txtConnectionStatus.Dispatcher.Invoke(new connectionParser(display_status), "Connected");

                        }
                        else
                        {

                            txtSerialInput.Dispatcher.Invoke(new connectionParser(display_input), "Bad Data!");
                        }

                    }
                    else
                    {
                        txtSerialInput.Dispatcher.Invoke(new connectionParser(display_input), "Connection N/A");
                        txtConnectionStatus.Dispatcher.Invoke(new connectionParser(display_status), "Disconnected");
                    }
                    Thread.Sleep(10);
                    ArduinoMode = (arduinoPort.IsOpen);
                }
                catch (Exception)
                {
                    //throw;
                }
            }
        }

        private void CursorRunner()
        {
            try
            {
                while (true)
                {
                    this.Dispatcher.Invoke(new AddGraphics(radarCursorAngleChange));

                    if (ArduinoMode) Thread.Sleep(25);
                    else Thread.Sleep(40);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        #endregion

        #region Drawing Functions

        private void drawArcPath()
        {
            foreach (System.Windows.Shapes.Path path in staticPathList)
            {
                lock (this)
                {
                    this.DrawCanvas.Children.Add(path);
                }
            }
            foreach (Label lbl in lblDistanceList)
            {
                lock (this)
                {
                    this.DrawCanvas.Children.Add(lbl);
                }
            }

        }

        private void drawStaticLines()
        {
            foreach (Line line1 in staticLineList)
            {
                lock (this)
                {
                    this.DrawCanvas.Children.Add(line1);
                }
            }
            foreach (Label lbl in lblAngleList)
            {
                lock (this)
                {
                    this.DrawCanvas.Children.Add(lbl);
                }
            }
        }

        public void drawStaticGraphics()
        {
            lock (this)
            {
                this.DrawCanvas.Children.Clear();
            }

            drawArcPath();
            drawStaticLines();
        }

        private void drawRadarRadius()
        {
            switch (ArduinoMode)
            {
                case false:
                    if (radarAngle < 180 && turnLeft) radarAngle++;
                    else if (radarAngle >= 180 || !turnLeft)
                    {
                        turnLeft = (radarAngle <= 0);
                        radarAngle--;
                    }
                    break;
                default:
                    break;
            }
            angleList[0] = radarAngle;
            for (int i = 30; i >= 1; i--)
            {
                angleList[i] = angleList[i - 1];
            }
            for (int i = 0; i <= 30; i++)
            {
                lock (this)
                {
                    if (radarRadius[i] != null)
                    {
                        this.DrawCanvas.Children.Remove(radarRadius[i]);
                    }

                    radarRadius[i].Opacity = 0.6 - 0.025 * i;
                    radarRadius[i].X2 = lineLength + Math.Cos(angleList[i] * Math.PI / 180) * (lineLength - 20);
                    radarRadius[i].Y2 = lineLength - Math.Sin(angleList[i] * Math.PI / 180) * (lineLength - 20);

                    this.DrawCanvas.Children.Add(radarRadius[i]);
                }
            }
            lblAngle.Content = "Radar Angle : " + angleList[0] + "°";
            lblDistance.Content = "Distance : " + distance + " m";
        }

        private void printAtia(double dist, int angle)
        {
            double scaleDistance;
            Point p = new Point();
            scaleDistance = map(0, 1, 0, lineLength, dist);
            p.X = lineLength + Math.Cos(angle * Math.PI / 180) * scaleDistance;
            p.Y = lineLength - Math.Sin(angle * Math.PI / 180) * scaleDistance;
            Atia atia = new Atia(this, p.X, p.Y);
            atia.Run(true);
        }

        private void printAtiaIfColizionExist()
        {
            if (!ArduinoMode)
            {
                Point p = Mouse.GetPosition(DrawCanvas);

                if (Math.Round(((p.Y - DrawCanvas.ActualHeight) / (p.X - DrawCanvas.ActualHeight)), 1) ==
                    Math.Round(((radarRadius[0].Y2 - DrawCanvas.ActualHeight) / (radarRadius[0].X2 - DrawCanvas.ActualHeight)), 1)
                    || ((int)p.X == (int)DrawCanvas.ActualHeight && (int)radarRadius[0].X2 == (int)DrawCanvas.ActualHeight))
                {
                    Atia atia = new Atia(this, p.X, p.Y);
                    atia.Run(false);
                }
            }
            else
            {

            }

        }

        #endregion

        #region Lists init

        private void radarRadiusInit()
        {
            for (int i = 0; i <= 30; i++)
            {
                radarRadius.Add(new Line());
                radarRadius[i].SnapsToDevicePixels = true;
                radarRadius[i].SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
                radarRadius[i].Visibility = Visibility.Visible;
                radarRadius[i].StrokeThickness = 5;
                radarRadius[i].Stroke = Brushes.LimeGreen;
                radarRadius[i].X1 = DrawCanvas.ActualWidth / 2;
                radarRadius[i].Y1 = DrawCanvas.ActualHeight;
                radarRadius[i].X2 = lineLength + Math.Cos(radarAngle * Math.PI / 180) * (lineLength - 20);
                radarRadius[i].Y2 = lineLength - Math.Sin(radarAngle * Math.PI / 180) * (lineLength - 20);
                angleList.Add(0);
            }
        }

        private void staticGraphInit()
        {
            double dblArcSizeDiv = lineLength / 10;
            double actualAngle = 0, outerArcRadius = lineLength + 2;
            int intDistanceCounter = 100, intLblCounter = 0;
            PathFigure ArcFigure = new PathFigure();

            if (DrawCanvas.ActualHeight > 0)
            {
                for (int i = 0; i <= 180; i++)
                {
                    staticLineList.Add(new Line());
                    staticLineList[i].SnapsToDevicePixels = true;
                    staticLineList[i].SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
                    staticLineList[i].Visibility = Visibility.Visible;
                    if (i == 0 || i == 90 || i == 180)
                    {
                        staticLineList[i].StrokeThickness = 5;
                        staticLineList[i].Opacity = 0.7;
                        staticLineList[i].X1 = DrawCanvas.ActualWidth / 2;
                        staticLineList[i].Y1 = DrawCanvas.ActualHeight;
                    }
                    else if (i % 10 == 0)
                    {
                        staticLineList[i].X1 = DrawCanvas.ActualWidth / 2;
                        staticLineList[i].Y1 = DrawCanvas.ActualHeight;
                        staticLineList[i].StrokeThickness = 2;
                        staticLineList[i].Opacity = 0.5;
                    }
                    else
                    {
                        staticLineList[i].X1 = lineLength + Math.Cos(actualAngle * Math.PI / 180) * (lineLength - 20);
                        staticLineList[i].Y1 = lineLength - Math.Sin(actualAngle * Math.PI / 180) * (lineLength - 20);
                        staticLineList[i].StrokeThickness = 1;
                        staticLineList[i].Opacity = 0.5;
                    }
                    staticLineList[i].Stroke = Brushes.LightGreen;
                    staticLineList[i].X2 = lineLength + Math.Cos(actualAngle * Math.PI / 180) * lineLength;
                    staticLineList[i].Y2 = lineLength - Math.Sin(actualAngle * Math.PI / 180) * lineLength;
                    if (i % 10 == 0)
                    {
                        lblAngleList.Add(new Label());
                        lblAngleList[intLblCounter].Content = actualAngle.ToString() + "°";
                        lblAngleList[intLblCounter].Foreground = new SolidColorBrush(Colors.White);
                        lblAngleList[intLblCounter].Background = new SolidColorBrush(Colors.Black);
                        if (actualAngle > 90)
                        {
                            lblAngleList[intLblCounter].Margin = new Thickness(staticLineList[i].X2 - lblAngleList[intLblCounter].ActualWidth - 5,
                                staticLineList[i].Y2 - lblAngleList[intLblCounter].ActualHeight, 0, 0);
                        }
                        else if (actualAngle == 90)
                        {
                            lblAngleList[intLblCounter].Margin = new Thickness(staticLineList[i].X2 - lblAngleList[intLblCounter].ActualWidth / 2,
                                staticLineList[i].Y2 - lblAngleList[intLblCounter].ActualHeight, 0, 0);
                        }
                        else
                        {
                            lblAngleList[intLblCounter].Margin = new Thickness(staticLineList[i].X2 + 5,
                                staticLineList[i].Y2 - lblAngleList[intLblCounter].ActualHeight, 0, 0);
                        }
                        intLblCounter++;
                    }

                    actualAngle += 1;
                }


                for (int i = 0; i <= 19; i++)
                {
                    lblDistanceList.Add(new Label());
                }

                for (int i = 0; i <= 9; i++)
                {
                    ArcFigure = new PathFigure();
                    dblArcSizeDiv = (lineLength) / 10 * i;
                    ArcFigure.StartPoint = new Point((DrawCanvas.ActualWidth - dblArcSizeDiv - 20), DrawCanvas.ActualHeight);
                    ArcFigure.Segments.Add(
                        new ArcSegment(
                            new Point(dblArcSizeDiv + 20, lineLength),
                                new Size((lineLength - dblArcSizeDiv - 20), (lineLength - dblArcSizeDiv - 20)),
                                90,
                                false,
                                SweepDirection.Counterclockwise,
                                    true));
                    PathGeometry ArcPath = new PathGeometry();
                    ArcPath.Figures.Add(ArcFigure);
                    staticPathList.Add(new System.Windows.Shapes.Path());
                    staticPathList[i].Data = ArcPath;
                    staticPathList[i].SnapsToDevicePixels = true;
                    staticPathList[i].SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
                    staticPathList[i].Visibility = Visibility.Visible;
                    staticPathList[i].Stroke = Brushes.LightGreen;
                    if (i == 0)
                    {
                        staticPathList[i].Opacity = 0.7;
                        staticPathList[i].StrokeThickness = 5;
                    }
                    else
                    {
                        staticPathList[i].Opacity = 0.3;
                        staticPathList[i].StrokeThickness = 2;
                    }

                    lblDistanceList[i].Content = intDistanceCounter.ToString() + " cm";
                    lblDistanceList[i].Foreground = new SolidColorBrush(Colors.White);
                    lblDistanceList[i].Background = new SolidColorBrush(Colors.Black);
                    lblDistanceList[i].Margin = new Thickness(ArcFigure.StartPoint.X - 20, ArcFigure.StartPoint.Y, 0, 0);
                    lblDistanceList[19 - i].Content = intDistanceCounter.ToString() + " cm";
                    lblDistanceList[19 - i].Foreground = new SolidColorBrush(Colors.White);
                    lblDistanceList[19 - i].Background = new SolidColorBrush(Colors.Black);
                    lblDistanceList[19 - i].Margin = new Thickness(dblArcSizeDiv, lineLength, 0, 0);
                    intDistanceCounter -= 10;

                }
            }


        }

        #endregion

        #region Functions / procedures

        private void handleConnection()
        {
            try
            {
                if (!arduinoPort.IsOpen)
                {
                    arduinoPort.PortName = txtSerialPort.Text;
                    arduinoPort.BaudRate = 9600;
                    arduinoPort.DataBits = 8;
                    arduinoPort.StopBits = StopBits.One;
                    arduinoPort.Parity = Parity.None;
                    arduinoPort.Open();
                    txtConnectionStatus.Text = "Connected";
                    btnConnect.Content = "Disconnect";
                    txtConnectionStatus.Background = Brushes.Green;
                    txtConnectionStatus.BorderBrush = Brushes.Green;
                }
                else
                {
                    arduinoPort.Close();
                    txtConnectionStatus.Text = "Disconnected";
                    btnConnect.Content = "Connect";
                    txtConnectionStatus.Background = Brushes.Red;
                    txtConnectionStatus.BorderBrush = Brushes.Red;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void radarCursorAngleChange()
        {
            if (this != null)
            {
                lineLength = DrawCanvas.ActualHeight;
                if (lineLength != 0)
                {
                    drawRadarRadius();
                    printAtiaIfColizionExist();
                    if (distance < 0.9 && ArduinoMode) printAtia(distance, radarAngle);
                }
            }
        }


        private double map(
            double originalStart, double originalEnd, // original range
            double newStart, double newEnd, // desired range
            double value) // value to convert
        {
            double scale = (double)(newEnd - newStart) / (originalEnd - originalStart);
            return (newStart + ((value - originalStart) * scale));
        }



        private void display_status(string state)
        {
            txtConnectionStatus.Background = (state == "Disconnected") ? Brushes.Red : Brushes.Green;
            txtConnectionStatus.Text = state;
        }

        private void display_input(string input)
        {
            txtSerialInput.IsEnabled = !(input == "Connection N/A");
            txtSerialInput.Text = input;
            txtSerialInput.Background = (input == "Bad Data!") ? Brushes.Red : Brushes.White;
        }

        #endregion

        #region Event Handlers

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //drawStaticGraphics();
            lineLength = DrawCanvas.ActualHeight;
            staticGraphInit();
            drawStaticGraphics();
            Run();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawCanvas.Height = DrawCanvas.Width / 2;
            lineLength = DrawCanvas.ActualHeight;
            staticGraphInit();
            radarRadiusInit();
            drawStaticGraphics();
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            handleConnection();
        }


        private void DrawCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p = Mouse.GetPosition(DrawCanvas);
            Atia atia = new Atia(this, p.X, p.Y);
            atia.Run(false);
        }

        #endregion
    }
}
