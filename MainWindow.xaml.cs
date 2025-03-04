using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STITimer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WindowState _previousWindowState;
        private WindowStyle _previousWindowStyle;
        private ResizeMode _previousResizeMode;
        private bool _isFullScreen;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F11)
            {
                ToggleFullScreen();
            }
        }

        private void Banner_Click(object sender, RoutedEventArgs e)
        {
            var logoWindow = new LogoWindow();
            logoWindow.ShowDialog();
        }

        private void Codefest_Click(object sender, RoutedEventArgs e)
        {
            var timerWindow = new TimerWindow("Images/Codefest.png", TimeSpan.FromHours(36));
            timerWindow.ShowDialog();
        }

        private void StandUpAndDeliver_Click(object sender, RoutedEventArgs e)
        {
            var trafficLight = new TrafficLightWindow("Images/Stand_Up_And_Deliver.png", 30, 120, 30);
            trafficLight.ShowDialog();
        }

        private void ThinkQuest_Click(object sender, RoutedEventArgs e)
        {
            var thinkQuest = new ThinkQuestWindow("Images/Think_Quest.png", 15);
            thinkQuest.ShowDialog();
        }

        // Stopwatch events
        private void ChefXpress_Click(object sender, RoutedEventArgs e)
        {
            var sw = new StopwatchWindow("Images/Chef_Xpress.png");
            sw.ShowDialog();
        }

        private void Travelogue_Click(object sender, RoutedEventArgs e)
        {
            var sw = new StopwatchWindow("Images/Travelogue.png");
            sw.ShowDialog();
        }

        private void MixAndFlair_Click(object sender, RoutedEventArgs e)
        {
            var sw = new StopwatchWindow("Images/Mix_&_Flair.png");
            sw.ShowDialog();
        }

        // Countdown events
        private void WorthTheWhisk_Click(object sender, RoutedEventArgs e)
        {
            var timer = new TimerWindow("Images/Worth_The_Whisk.png", TimeSpan.FromHours(3));
            timer.ShowDialog();
        }

        private void EssayDoesIt_Click(object sender, RoutedEventArgs e)
        {
            var timer = new TimerWindow("Images/Essay_Does_It.png", TimeSpan.FromHours(1));
            timer.ShowDialog();
        }

        private void AllInPlace_Click(object sender, RoutedEventArgs e)
        {
            var timer = new TimerWindow("Images/All_In_Place.png", TimeSpan.FromMinutes(30));
            timer.ShowDialog();
        }

        private void ToggleFullScreen()
        {
            if (_isFullScreen)
            {
                WindowStyle = _previousWindowStyle;
                ResizeMode = _previousResizeMode;
                WindowState = _previousWindowState;
                _isFullScreen = false;
            }
            else
            {
                _previousWindowState = WindowState;
                _previousWindowStyle = WindowStyle;
                _previousResizeMode = ResizeMode;

                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                WindowState = WindowState.Maximized;
                _isFullScreen = true;
            }
        }
    }
}
