using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Arduino_Radar_UI.Net
{
    public class Atia
    {
        #region General Declaration

        private delegate void AddPicture();

        private Thread targetRunner;
        public bool visible;
        private MainWindow window;
        private double locationX, locationY;
        public int score = 0;
        Ellipse atia = new Ellipse();

        #endregion

        #region Constractor

        public Atia(MainWindow window, double locX, double locY)
        {
            this.window = window;
            //get 2 realy random number for x,y
            locationX = locX;
            locationY = locY;
            visible = true;
            //Run();
        }

        #endregion

        #region Procedures / Functions

        private void addAtia()
        {
            atia.SnapsToDevicePixels = true;
            atia.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            atia.Visibility = Visibility.Visible;
            atia.Stroke = Brushes.OrangeRed;
            atia.Fill = Brushes.OrangeRed;
            atia.StrokeThickness =4.5;
            atia.Height = 4.5;
            atia.Width = 4.5;
            atia.Margin = new Thickness((locationX - (atia.Width / 2)),
                (locationY - (atia.Height / 2)), locationX + atia.Width - (atia.Width / 2),
                (locationY + atia.Height - (atia.Height / 2)));

            if (atia != null) window.DrawCanvas.Children.Add(atia);
        }

        private void removeAtia()
        {
            if (atia != null) window.DrawCanvas.Children.Remove(atia);
            //atia = null;
        }

        private void AtiaSizeChange()
        {
            try
            {
                if (atia != null)
                {
                    atia.Width += 0.75;
                    atia.Height += 0.75;
                    atia.Margin = new Thickness(
                        (locationX - (atia.Width / 2)),
                            (locationY - (atia.Height / 2)),
                            (locationX + atia.Width + (atia.Width / 2)),
                            (locationY + atia.Height + (atia.Height / 2)));
                    atia.Opacity -= 0.01;
                    visible = (atia.Opacity >= 0.01);
                }
            }
            catch (Exception)
            {
                
            }
            
        }

        private bool clockEnded()
        {
            if (!visible)
            {
                window.Dispatcher.Invoke(new AddPicture(removeAtia));
            }
            return (!visible);
        }

        #endregion

        #region Thread's Functions

        public void Run(bool ArduinoMode)
        {
            targetRunner = new Thread(new ThreadStart(runner));
            targetRunner.IsBackground = true;
            targetRunner.Start();
            window.Dispatcher.Invoke(new AddPicture(addAtia));
            if (!ArduinoMode)
            {
                SoundPlayer player = new SoundPlayer(Properties.Resources._18419_sonar);
                player.Load();
                player.Play();
            }

        }

        private void runner()
        {
            try
            {
                while (!clockEnded())
                {
                    window.Dispatcher.Invoke(new AddPicture(AtiaSizeChange));
                    Thread.Sleep(90);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        #endregion


    }
}
