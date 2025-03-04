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
    public partial class TrafficLightWindow : Window
    {
        private readonly DispatcherTimer _timer;
        private readonly System.Diagnostics.Stopwatch _sw = new System.Diagnostics.Stopwatch();
        private TimeSpan _prepDuration;
        private TimeSpan _speechDuration;
        private readonly TimeSpan _yellowThreshold;
        private TimeSpan _remaining;
        private TimeSpan _remainingAtStart;
        private bool _inPrepPhase;
        private bool _isRunning;
        private bool _isClosing;
        private bool _countdownStarted;
        private bool _blinkState;
        private int _currentPhase;
        private DispatcherTimer _blinkTimer;
        private SolidColorBrush _overlayBrush;

        private static readonly SolidColorBrush PrepBrush;
        private static readonly SolidColorBrush GreenBrush;
        private static readonly SolidColorBrush YellowBrush;
        private static readonly SolidColorBrush RedBrush;
        private static readonly SolidColorBrush BlinkRedBrush;
        private static readonly SolidColorBrush BlinkWhiteOverlayBrush;

        static TrafficLightWindow()
        {
            PrepBrush = new SolidColorBrush(Color.FromArgb(0xCC, 0x15, 0x65, 0xC0));
            PrepBrush.Freeze();
            GreenBrush = new SolidColorBrush(Color.FromArgb(0xCC, 0x2E, 0x7D, 0x32));
            GreenBrush.Freeze();
            YellowBrush = new SolidColorBrush(Color.FromArgb(0xCC, 0xF9, 0xA8, 0x25));
            YellowBrush.Freeze();
            RedBrush = new SolidColorBrush(Color.FromArgb(0xCC, 0xC6, 0x28, 0x28));
            RedBrush.Freeze();
            BlinkRedBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0x17, 0x44));
            BlinkRedBrush.Freeze();
            BlinkWhiteOverlayBrush = new SolidColorBrush(Color.FromArgb(0xCC, 0xFF, 0xFF, 0xFF));
            BlinkWhiteOverlayBrush.Freeze();
        }

        public TrafficLightWindow(string eventImagePath, int prepSeconds, int speechSeconds, int yellowSeconds)
        {
            InitializeComponent();

            _overlayBrush = new SolidColorBrush(PrepBrush.Color);
            PhaseOverlay.Background = _overlayBrush;

            _prepDuration = TimeSpan.FromSeconds(prepSeconds);
            _speechDuration = TimeSpan.FromSeconds(speechSeconds);
            _yellowThreshold = TimeSpan.FromSeconds(yellowSeconds);

            EventImage.Source = new BitmapImage(new Uri("pack://application:,,,/" + eventImagePath));

            PrepSecondsBox.Text = prepSeconds.ToString();
            SpeechMinutesBox.Text = (speechSeconds / 60).ToString();
            SpeechSecondsBox.Text = (speechSeconds % 60).ToString("D2");

            _currentPhase = -1;
            ResetTimer();

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            _timer.Tick += Timer_Tick;

            _blinkTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _blinkTimer.Tick += Blink_Tick;

            PrepSecondsBox.TextChanged += Config_TextChanged;
            SpeechMinutesBox.TextChanged += Config_TextChanged;
            SpeechSecondsBox.TextChanged += Config_TextChanged;

            PrepSecondsBox.PreviewTextInput += NumericOnly_PreviewTextInput;
            SpeechMinutesBox.PreviewTextInput += NumericOnly_PreviewTextInput;
            SpeechSecondsBox.PreviewTextInput += NumericOnly_PreviewTextInput;

            DataObject.AddPastingHandler(PrepSecondsBox, NumericOnly_Pasting);
            DataObject.AddPastingHandler(SpeechMinutesBox, NumericOnly_Pasting);
            DataObject.AddPastingHandler(SpeechSecondsBox, NumericOnly_Pasting);

            PrepSecondsBox.LostFocus += ClampToMax60_LostFocus;
            SpeechMinutesBox.LostFocus += ClampToMax60_LostFocus;
            SpeechSecondsBox.LostFocus += ClampToMax60_LostFocus;

            SkipPrepCheck.Checked += SkipPrepCheck_Changed;
            SkipPrepCheck.Unchecked += SkipPrepCheck_Changed;
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

        private void SkipPrepCheck_Changed(object sender, RoutedEventArgs e)
        {
            if (_isRunning) return;
            bool skip = SkipPrepCheck.IsChecked == true;
            PrepSecondsBox.IsEnabled = !skip;
            PrepLabel.Opacity = skip ? 0.4 : 1.0;
            PrepUnitLabel.Opacity = skip ? 0.4 : 1.0;
            if (TryReadConfig())
            {
                ResetTimer();
            }
        }

        private void Config_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isRunning) return;
            if (TryReadConfig())
            {
                _inPrepPhase = true;
                _remaining = _prepDuration;
                UpdateDisplay();
                _currentPhase = -1;
                UpdatePhase();
            }
        }

        private bool TryReadConfig()
        {
            bool skipPrep = SkipPrepCheck != null && SkipPrepCheck.IsChecked == true;

            if (!skipPrep)
            {
                if (!int.TryParse(PrepSecondsBox.Text, out int prepSec) || prepSec <= 0) return false;
                if (prepSec > 60) prepSec = 60;
                _prepDuration = TimeSpan.FromSeconds(prepSec);
            }
            else
            {
                _prepDuration = TimeSpan.Zero;
            }

            if (!int.TryParse(SpeechMinutesBox.Text, out int speechMin) || speechMin < 0) return false;
            if (!int.TryParse(SpeechSecondsBox.Text, out int speechSec) || speechSec < 0) return false;
            if (speechMin > 60) speechMin = 60;
            if (speechSec > 60) speechSec = 60;
            if (speechMin == 0 && speechSec == 0) return false;

            _speechDuration = TimeSpan.FromSeconds(speechMin * 60 + speechSec);
            return true;
        }

        private void ResetTimer()
        {
            bool skipPrep = SkipPrepCheck != null && SkipPrepCheck.IsChecked == true;
            if (skipPrep)
            {
                _inPrepPhase = false;
                _remaining = _speechDuration;
            }
            else
            {
                _inPrepPhase = true;
                _remaining = _prepDuration;
            }
            UpdateDisplay();
            _currentPhase = -1;
            UpdatePhase();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            TimeSpan remaining = _remainingAtStart - _sw.Elapsed;
            if (remaining.TotalSeconds > 0)
            {
                _remaining = remaining;
                UpdateDisplay();
                UpdatePhase();

                if (!_inPrepPhase && _remaining.TotalSeconds <= 10 && !_countdownStarted)
                {
                    _countdownStarted = true;
                }

                if (_countdownStarted)
                {
                    int sec = (int)Math.Ceiling(_remaining.TotalSeconds);
                    SetTimerForeground(sec % 2 == 0 ? BlinkRedBrush : Brushes.White);
                }
            }
            else if (_inPrepPhase)
            {
                _inPrepPhase = false;
                _remaining = _speechDuration;
                _remainingAtStart = _speechDuration;
                _sw.Reset();
                _sw.Start();
                StopCountdownEffects();
                UpdateDisplay();
                UpdatePhase();
            }
            else
            {
                _sw.Stop();
                _timer.Stop();
                _isRunning = false;
                _remaining = TimeSpan.Zero;
                _countdownStarted = false;
                SetTimerForeground(Brushes.Black);
                _blinkState = false;
                _blinkTimer.Start();
                UpdateDisplay();
                UpdatePhase();
                StartPauseButton.Content = "▶  Start";
                SetConfigEditable(true);
            }
        }

        private void UpdateDisplay()
        {
            MinutesDisplay.Text = ((int)_remaining.TotalMinutes).ToString("D2");
            SecondsDisplay.Text = _remaining.Seconds.ToString("D2");
        }

        private void UpdatePhase()
        {
            int newPhase;
            if (_inPrepPhase)
                newPhase = 0;
            else if (_remaining.TotalSeconds > _yellowThreshold.TotalSeconds)
                newPhase = 1;
            else if (_remaining.TotalSeconds > 0)
                newPhase = 2;
            else
                newPhase = 3;

            if (newPhase == _currentPhase) return;
            bool isInitial = _currentPhase < 0;
            _currentPhase = newPhase;

            string label;
            Color targetColor;
            switch (newPhase)
            {
                case 0: label = "PREPARATION"; targetColor = PrepBrush.Color; break;
                case 1: label = "SPEAKING"; targetColor = GreenBrush.Color; break;
                case 2: label = "WRAP UP"; targetColor = YellowBrush.Color; break;
                case 3: label = "TIME'S UP"; targetColor = RedBrush.Color; break;
                default: return;
            }

            if (isInitial)
                PhaseLabel.Text = label;
            else
                AnimatePhaseLabel(label);

            AnimateOverlayColor(targetColor);

            if (newPhase == 3)
                PhaseLabel.Foreground = Brushes.Black;
        }

        private void ReadConfig()
        {
            bool skipPrep = SkipPrepCheck.IsChecked == true;

            if (!skipPrep)
            {
                int.TryParse(PrepSecondsBox.Text, out int prepSec);
                if (prepSec > 60) prepSec = 60;
                _prepDuration = TimeSpan.FromSeconds(Math.Max(prepSec, 1));
            }
            else
            {
                _prepDuration = TimeSpan.Zero;
            }

            int.TryParse(SpeechMinutesBox.Text, out int speechMin);
            int.TryParse(SpeechSecondsBox.Text, out int speechSec);
            if (speechMin > 60) speechMin = 60;
            if (speechSec > 60) speechSec = 60;
            _speechDuration = TimeSpan.FromSeconds(Math.Max(speechMin * 60 + speechSec, 1));
        }

        private void ToggleStartPause()
        {
            if (_isRunning)
            {
                _sw.Stop();
                _remaining = _remainingAtStart - _sw.Elapsed;
                if (_remaining.TotalSeconds < 0) _remaining = TimeSpan.Zero;
                _sw.Reset();
                _timer.Stop();
                _isRunning = false;
                StopCountdownEffects();
                UpdateDisplay();
                StartPauseButton.Content = "▶  Start";
                SetConfigEditable(true);
            }
            else
            {
                if (_remaining.TotalSeconds <= 0 && !_inPrepPhase)
                {
                    ReadConfig();
                    ResetTimer();
                }

                if (!_inPrepPhase && _remaining.TotalSeconds <= 10)
                {
                    StartCountdownEffects();
                }

                _remainingAtStart = _remaining;
                _sw.Reset();
                _sw.Start();
                _timer.Start();
                _isRunning = true;
                StartPauseButton.Content = "⏸  Pause";
                SetConfigEditable(false);
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
            ReadConfig();
            ResetTimer();
            StartPauseButton.Content = "▶  Start";
            SetConfigEditable(true);
        }

        private void SetConfigEditable(bool editable)
        {
            PrepSecondsBox.IsReadOnly = !editable;
            SpeechMinutesBox.IsReadOnly = !editable;
            SpeechSecondsBox.IsReadOnly = !editable;
        }

        private void ClampToMax60_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = (TextBox)sender;
            if (int.TryParse(tb.Text, out int val) && val > 60)
            {
                tb.Text = "60";
            }
        }

        private void StartCountdownEffects()
        {
            _countdownStarted = true;
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

        private void AnimatePhaseLabel(string newText)
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.12));
            fadeOut.Completed += (s, _) =>
            {
                PhaseLabel.Text = newText;
                PhaseLabel.BeginAnimation(OpacityProperty,
                    new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.18)));
            };
            PhaseLabel.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void StopCountdownEffects()
        {
            _blinkTimer.Stop();
            _blinkState = false;
            _countdownStarted = false;
            _overlayBrush.BeginAnimation(SolidColorBrush.ColorProperty, null);
            SetTimerForeground(Brushes.White);
            PhaseLabel.Foreground = Brushes.White;
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
                _overlayBrush.Color = RedBrush.Color;
                StartPauseButton.Foreground = Brushes.White;
                ResetButton.Foreground = Brushes.White;
                HintText.Foreground = Brushes.White;
            }
        }

        private void SetTimerForeground(Brush brush)
        {
            MinutesDisplay.Foreground = brush;
            SeparatorDisplay.Foreground = brush;
            SecondsDisplay.Foreground = brush;
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
