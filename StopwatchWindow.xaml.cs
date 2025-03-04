using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace STITimer
{
    public partial class StopwatchWindow : Window
    {
        private readonly DispatcherTimer _timer;
        private readonly System.Diagnostics.Stopwatch _sw = new System.Diagnostics.Stopwatch();
        private TimeSpan _accumulatedElapsed;
        private bool _isRunning;
        private bool _isClosing;

        public StopwatchWindow(string eventImagePath)
        {
            InitializeComponent();

            _accumulatedElapsed = TimeSpan.Zero;
            EventImage.Source = new BitmapImage(new Uri("pack://application:,,,/" + eventImagePath));
            UpdateDisplay();

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            _timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            TimeSpan elapsed = _accumulatedElapsed + _sw.Elapsed;
            HoursDisplay.Text = ((int)elapsed.TotalHours).ToString("D2");
            MinutesDisplay.Text = elapsed.Minutes.ToString("D2");
            SecondsDisplay.Text = elapsed.Seconds.ToString("D2");
        }

        private void ToggleStartPause()
        {
            if (_isRunning)
            {
                _sw.Stop();
                _accumulatedElapsed += _sw.Elapsed;
                _sw.Reset();
                _timer.Stop();
                _isRunning = false;
                UpdateDisplay();
                StartPauseButton.Content = "▶  Start";
            }
            else
            {
                _sw.Start();
                _timer.Start();
                _isRunning = true;
                StartPauseButton.Content = "⏸  Pause";
            }
        }

        private void StartPause_Click(object sender, RoutedEventArgs e)
        {
            ToggleStartPause();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            _sw.Stop();
            _sw.Reset();
            _timer.Stop();
            _isRunning = false;
            _accumulatedElapsed = TimeSpan.Zero;
            UpdateDisplay();
            StartPauseButton.Content = "▶  Start";
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && !_isClosing)
            {
                _isClosing = true;
                _sw.Stop();
                _timer.Stop();
                var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.35))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
                };
                fadeOut.Completed += (s, args) => Close();
                BeginAnimation(OpacityProperty, fadeOut);
            }
            else if (e.Key == Key.Space && !_isClosing)
            {
                ToggleStartPause();
                e.Handled = true;
            }
            else if (e.Key == Key.R && !_isClosing)
            {
                Reset_Click(sender, e);
                e.Handled = true;
            }
        }
    }
}
