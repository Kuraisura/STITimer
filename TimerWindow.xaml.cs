using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace STITimer
{
    public partial class TimerWindow : Window
    {
        private readonly DispatcherTimer _timer;
        private readonly DispatcherTimer _blinkTimer;
        private readonly System.Diagnostics.Stopwatch _sw = new System.Diagnostics.Stopwatch();
        private readonly TimeSpan _defaultTime;
        private TimeSpan _remainingTime;
        private TimeSpan _remainingAtStart;
        private bool _isRunning;
        private bool _isClosing;
        private bool _countdownStarted;
        private bool _blinkState;
        private SolidColorBrush _overlayBrush;

        private static readonly SolidColorBrush NormalOverlayBrush;
        private static readonly SolidColorBrush RedOverlayBrush;
        private static readonly SolidColorBrush BlinkRedBrush;
        private static readonly SolidColorBrush BlinkWhiteOverlayBrush;

        static TimerWindow()
        {
            NormalOverlayBrush = new SolidColorBrush(Color.FromArgb(0xAA, 0x00, 0x00, 0x00));
            NormalOverlayBrush.Freeze();
            RedOverlayBrush = new SolidColorBrush(Color.FromArgb(0xCC, 0xC6, 0x28, 0x28));
            RedOverlayBrush.Freeze();
            BlinkRedBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0x17, 0x44));
            BlinkRedBrush.Freeze();
            BlinkWhiteOverlayBrush = new SolidColorBrush(Color.FromArgb(0xCC, 0xFF, 0xFF, 0xFF));
            BlinkWhiteOverlayBrush.Freeze();
        }

        public TimerWindow(string eventImagePath, TimeSpan defaultTime)
        {
            InitializeComponent();

            _overlayBrush = new SolidColorBrush(NormalOverlayBrush.Color);
            Overlay.Background = _overlayBrush;

            _defaultTime = defaultTime;
            _remainingTime = defaultTime;

            EventImage.Source = new BitmapImage(new Uri("pack://application:,,,/" + eventImagePath));

            UpdateDisplay();

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            _timer.Tick += Timer_Tick;

            _blinkTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _blinkTimer.Tick += Blink_Tick;

            HoursBox.PreviewTextInput += NumericOnly_PreviewTextInput;
            MinutesBox.PreviewTextInput += NumericOnly_PreviewTextInput;
            SecondsBox.PreviewTextInput += NumericOnly_PreviewTextInput;

            DataObject.AddPastingHandler(HoursBox, NumericOnly_Pasting);
            DataObject.AddPastingHandler(MinutesBox, NumericOnly_Pasting);
            DataObject.AddPastingHandler(SecondsBox, NumericOnly_Pasting);

            MinutesBox.LostFocus += ClampToMax60_LostFocus;
            SecondsBox.LostFocus += ClampToMax60_LostFocus;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            TimeSpan remaining = _remainingAtStart - _sw.Elapsed;
            if (remaining.TotalSeconds > 0)
            {
                _remainingTime = remaining;
                UpdateDisplay();

                if (_remainingTime.TotalSeconds <= 10 && !_countdownStarted)
                {
                    _countdownStarted = true;
                }

                if (_countdownStarted)
                {
                    int sec = (int)Math.Ceiling(_remainingTime.TotalSeconds);
                    SetTimerForeground(sec % 2 == 0 ? BlinkRedBrush : Brushes.White);
                }
            }
            else
            {
                _sw.Stop();
                _timer.Stop();
                _isRunning = false;
                _remainingTime = TimeSpan.Zero;
                UpdateDisplay();
                _countdownStarted = false;
                SetTimerForeground(Brushes.Black);
                _blinkState = false;
                _blinkTimer.Start();
                AnimateOverlayColor(RedOverlayBrush.Color);
                StatusLabel.Foreground = Brushes.Black;
                ShowStatusLabel();
                StartPauseButton.Content = "▶  Start";
                SetEditable(true);
            }
        }

        private void UpdateDisplay()
        {
            HoursBox.Text = ((int)_remainingTime.TotalHours).ToString("D2");
            MinutesBox.Text = _remainingTime.Minutes.ToString("D2");
            SecondsBox.Text = _remainingTime.Seconds.ToString("D2");
        }

        private TimeSpan ReadTimeFromBoxes()
        {
            int.TryParse(HoursBox.Text, out int hours);
            int.TryParse(MinutesBox.Text, out int minutes);
            int.TryParse(SecondsBox.Text, out int seconds);
            if (minutes > 60) minutes = 60;
            if (seconds > 60) seconds = 60;
            return new TimeSpan(hours, minutes, seconds);
        }

        private void ToggleStartPause()
        {
            if (_isRunning)
            {
                _sw.Stop();
                _remainingTime = _remainingAtStart - _sw.Elapsed;
                if (_remainingTime.TotalSeconds < 0) _remainingTime = TimeSpan.Zero;
                _sw.Reset();
                _timer.Stop();
                _isRunning = false;
                StopCountdownEffects();
                UpdateDisplay();
                StartPauseButton.Content = "▶  Start";
                SetEditable(true);
            }
            else
            {
                _remainingTime = ReadTimeFromBoxes();
                if (_remainingTime.TotalSeconds <= 0) return;

                AnimateOverlayColor(NormalOverlayBrush.Color);
                StatusLabel.Visibility = Visibility.Collapsed;

                if (_remainingTime.TotalSeconds <= 10)
                {
                    StartCountdownEffects();
                }

                _remainingAtStart = _remainingTime;
                _sw.Reset();
                _sw.Start();
                _timer.Start();
                _isRunning = true;
                StartPauseButton.Content = "⏸  Pause";
                SetEditable(false);
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
            StopCountdownEffects();
            _remainingTime = _defaultTime;
            UpdateDisplay();
            AnimateOverlayColor(NormalOverlayBrush.Color);
            StatusLabel.Visibility = Visibility.Collapsed;
            StartPauseButton.Content = "▶  Start";
            SetEditable(true);
        }

        private void SetEditable(bool editable)
        {
            HoursBox.IsReadOnly = !editable;
            MinutesBox.IsReadOnly = !editable;
            SecondsBox.IsReadOnly = !editable;
        }

        private void NumericOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c))
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void NumericOnly_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                foreach (char c in text)
                {
                    if (!char.IsDigit(c))
                    {
                        e.CancelCommand();
                        return;
                    }
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void ClampToMax60_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = (TextBox)sender;
            if (int.TryParse(tb.Text, out int val) && val > 60)
            {
                tb.Text = "60";
            }
        }

        private void AnimateOverlayColor(Color targetColor)
        {
            _overlayBrush.BeginAnimation(SolidColorBrush.ColorProperty, null);
            var anim = new ColorAnimation(targetColor, TimeSpan.FromSeconds(0.4))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
            _overlayBrush.BeginAnimation(SolidColorBrush.ColorProperty, anim);
        }

        private void ShowStatusLabel()
        {
            StatusLabel.RenderTransformOrigin = new Point(0.5, 0.5);
            StatusLabel.RenderTransform = new ScaleTransform(0.5, 0.5);
            StatusLabel.Opacity = 0;
            StatusLabel.Visibility = Visibility.Visible;
            var ease = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.4 };
            var dur = TimeSpan.FromSeconds(0.35);
            StatusLabel.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty,
                new DoubleAnimation(0.5, 1.0, dur) { EasingFunction = ease });
            StatusLabel.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty,
                new DoubleAnimation(0.5, 1.0, dur) { EasingFunction = ease });
            StatusLabel.BeginAnimation(OpacityProperty,
                new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.25)));
        }

        private void StartCountdownEffects()
        {
            _countdownStarted = true;
        }

        private void StopCountdownEffects()
        {
            _blinkTimer.Stop();
            _blinkState = false;
            _countdownStarted = false;
            _overlayBrush.BeginAnimation(SolidColorBrush.ColorProperty, null);
            _overlayBrush.Color = NormalOverlayBrush.Color;
            SetTimerForeground(Brushes.White);
            StatusLabel.Foreground = Brushes.White;
            StartPauseButton.Foreground = Brushes.White;
            ResetButton.Foreground = Brushes.White;
            HintText.Foreground = new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF));
        }

        private void Blink_Tick(object sender, EventArgs e)
        {
            _blinkState = !_blinkState;
            _overlayBrush.BeginAnimation(SolidColorBrush.ColorProperty, null);
            if (_blinkState)
            {
                _overlayBrush.Color = BlinkWhiteOverlayBrush.Color;
                StartPauseButton.Foreground = Brushes.Black;
                ResetButton.Foreground = Brushes.Black;
                HintText.Foreground = Brushes.Black;
            }
            else
            {
                _overlayBrush.Color = RedOverlayBrush.Color;
                StartPauseButton.Foreground = Brushes.White;
                ResetButton.Foreground = Brushes.White;
                HintText.Foreground = Brushes.White;
            }
        }

        private void SetTimerForeground(Brush brush)
        {
            HoursBox.Foreground = brush;
            Separator1.Foreground = brush;
            MinutesBox.Foreground = brush;
            Separator2.Foreground = brush;
            SecondsBox.Foreground = brush;
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
                if (!(Keyboard.FocusedElement is TextBox))
                {
                    ToggleStartPause();
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.R && !_isClosing)
            {
                if (!(Keyboard.FocusedElement is TextBox))
                {
                    Reset_Click(sender, e);
                    e.Handled = true;
                }
            }
        }
    }
}
