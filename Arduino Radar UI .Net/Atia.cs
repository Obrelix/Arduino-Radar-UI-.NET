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
        public bool isEnded;
        private MainWindow window;
        private int clockCounter, lifeTime;
        private double locationX, locationY;
        public int score = 0;
        Ellipse atia = new Ellipse();

        #endregion

        #region Constractor

        public Atia(MainWindow window, double locX, double locY)
        {
            lifeTime = 200;
            isEnded = false;
            this.window = window;
            //get 2 realy random number for x,y
            locationX = locX;
            locationY = locY;
            clockCounter = 0;
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
            atia.StrokeThickness = 5;
            atia.Height = 5;
            atia.Width = 5;
            atia.Margin = new Thickness((locationX - (atia.Width / 2)),
                (locationY - (atia.Height / 2)), locationX + atia.Width - (atia.Width / 2),
                (locationY + atia.Height - (atia.Height / 2)));

            if (atia != null) window.DrawCanvas.Children.Add(atia);
        }

        private void removeAtia()
        {
            atia.Opacity = 0;
            if (atia != null) window.DrawCanvas.Children.Remove(atia);
            atia = null;
        }

        private void AtiaSizeChange()
        {
            if (atia != null)
            {
                atia.Width += 1;
                atia.Height += 1;
                atia.Margin = new Thickness(
                    (locationX - (atia.Width / 2)),
                     (locationY - (atia.Height / 2)),
                      (locationX + atia.Width + (atia.Width / 2)),
                       (locationY + atia.Height + (atia.Height / 2)));
                atia.Opacity -= 0.01;

            }
        }

        private bool clockEnded()
        {
            if (clockCounter > lifeTime)
            {
                window.Dispatcher.Invoke(new AddPicture(removeAtia));
            }
            return (clockCounter > lifeTime);
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
                    clockCounter++;
                    window.Dispatcher.Invoke(new AddPicture(AtiaSizeChange));
                    Thread.Sleep(80);
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
