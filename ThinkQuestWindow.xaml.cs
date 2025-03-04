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
    public partial class ThinkQuestWindow : Window
    {
        private readonly DispatcherTimer _timer;
        private readonly DispatcherTimer _blinkTimer;
        private readonly System.Diagnostics.Stopwatch _sw = new System.Diagnostics.Stopwatch();
        private TimeSpan _selectedTime;
        private TimeSpan _remaining;
        private TimeSpan _remainingAtStart;
        private bool _isRunning;
        private bool _isClosing;
        private bool _blinkState;
        private bool _countdownStarted;
        private int _selectedPresetIndex;
        private SolidColorBrush _overlayBrush;

        private Border[] _presetCards;
        private TextBox[] _presetTimeBoxes;

        private static readonly SolidColorBrush NormalOverlayBrush;
        private static readonly SolidColorBrush RedOverlayBrush;
        private static readonly SolidColorBrush BlinkRedBrush;
        private static readonly SolidColorBrush BlinkWhiteOverlayBrush;
        private static readonly SolidColorBrush[] NormalCardBrushes;
        private static readonly SolidColorBrush[] SelectedCardBrushes;
        private static readonly SolidColorBrush[] CardBorderBrushes;

        static ThinkQuestWindow()
        {
            NormalOverlayBrush = new SolidColorBrush(Color.FromArgb(0xAA, 0x00, 0x00, 0x00));
            NormalOverlayBrush.Freeze();
            RedOverlayBrush = new SolidColorBrush(Color.FromArgb(0xCC, 0xC6, 0x28, 0x28));
            RedOverlayBrush.Freeze();
            BlinkRedBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0x17, 0x44));
            BlinkRedBrush.Freeze();
            BlinkWhiteOverlayBrush = new SolidColorBrush(Color.FromArgb(0xCC, 0xFF, 0xFF, 0xFF));
            BlinkWhiteOverlayBrush.Freeze();
            NormalCardBrushes = new SolidColorBrush[3];
            SelectedCardBrushes = new SolidColorBrush[3];
            CardBorderBrushes = new SolidColorBrush[3];

            NormalCardBrushes[0] = new SolidColorBrush(Color.FromArgb(0x44, 0x2E, 0x7D, 0x32));
            NormalCardBrushes[0].Freeze();
            SelectedCardBrushes[0] = new SolidColorBrush(Color.FromArgb(0x88, 0x2E, 0x7D, 0x32));
            SelectedCardBrushes[0].Freeze();
            CardBorderBrushes[0] = new SolidColorBrush(Color.FromArgb(0x99, 0x4C, 0xAF, 0x50));
            CardBorderBrushes[0].Freeze();

            NormalCardBrushes[1] = new SolidColorBrush(Color.FromArgb(0x44, 0xFF, 0x8F, 0x00));
            NormalCardBrushes[1].Freeze();
            SelectedCardBrushes[1] = new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0x8F, 0x00));
            SelectedCardBrushes[1].Freeze();
            CardBorderBrushes[1] = new SolidColorBrush(Color.FromArgb(0x99, 0xFF, 0xB3, 0x00));
            CardBorderBrushes[1].Freeze();

            NormalCardBrushes[2] = new SolidColorBrush(Color.FromArgb(0x44, 0xC6, 0x28, 0x28));
            NormalCardBrushes[2].Freeze();
            SelectedCardBrushes[2] = new SolidColorBrush(Color.FromArgb(0x88, 0xC6, 0x28, 0x28));
            SelectedCardBrushes[2].Freeze();
            CardBorderBrushes[2] = new SolidColorBrush(Color.FromArgb(0x99, 0xEF, 0x53, 0x50));
            CardBorderBrushes[2].Freeze();
        }

        public ThinkQuestWindow(string eventImagePath, int defaultSeconds)
        {
            InitializeComponent();

            _overlayBrush = new SolidColorBrush(NormalOverlayBrush.Color);
            Overlay.Background = _overlayBrush;

            _selectedTime = TimeSpan.FromSeconds(defaultSeconds);
            _remaining = _selectedTime;

            EventImage.Source = new BitmapImage(new Uri("pack://application:,,,/" + eventImagePath));

            _presetCards = new[] { EasyCard, MediumCard, HardCard };
            _presetTimeBoxes = new[] { EasyTimeBox, MediumTimeBox, HardTimeBox };

            _selectedPresetIndex = 0;
            UpdateDisplay();
            HighlightPreset(0);

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            _timer.Tick += Timer_Tick;

            _blinkTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _blinkTimer.Tick += Blink_Tick;

            foreach (var tb in _presetTimeBoxes)
            {
                tb.TextChanged += PresetTime_TextChanged;
                tb.PreviewTextInput += NumericOnly_PreviewTextInput;
                DataObject.AddPastingHandler(tb, NumericOnly_Pasting);
                tb.LostFocus += ClampToMax60_LostFocus;
            }
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

        private void PresetTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isRunning) return;
            var tb = (TextBox)sender;
            int index = Array.IndexOf(_presetTimeBoxes, tb);
            if (index == _selectedPresetIndex && int.TryParse(tb.Text, out int sec) && sec > 0)
            {
                _selectedTime = TimeSpan.FromSeconds(sec);
                _remaining = _selectedTime;
                UpdateDisplay();
            }
        }

        private void HighlightPreset(int index)
        {
            for (int i = 0; i < _presetCards.Length; i++)
            {
                _presetCards[i].Background = (i == index) ? SelectedCardBrushes[i] : NormalCardBrushes[i];
                _presetCards[i].BorderBrush = CardBorderBrushes[i];
            }
            _selectedPresetIndex = index;
        }

        private void SelectPreset(int index)
        {
            if (_isRunning) return;
            if (int.TryParse(_presetTimeBoxes[index].Text, out int sec) && sec > 0)
            {
                _selectedTime = TimeSpan.FromSeconds(sec);
                ResetToSelected();
            }
            HighlightPreset(index);
        }

        private void EasyCard_Click(object sender, MouseButtonEventArgs e) { SelectPreset(0); }
        private void MediumCard_Click(object sender, MouseButtonEventArgs e) { SelectPreset(1); }
        private void HardCard_Click(object sender, MouseButtonEventArgs e) { SelectPreset(2); }

        private void ResetToSelected()
        {
            _remaining = _selectedTime;
            _countdownStarted = false;
            UpdateDisplay();
            AnimateOverlayColor(NormalOverlayBrush.Color);
            StatusLabel.Visibility = Visibility.Collapsed;
            StopCountdownEffects();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            TimeSpan remaining = _remainingAtStart - _sw.Elapsed;
            if (remaining.TotalSeconds > 0)
            {
                _remaining = remaining;
                UpdateDisplay();

                if (_remaining.TotalSeconds <= 10 && !_countdownStarted)
                {
                    _countdownStarted = true;
                }

                if (_countdownStarted)
                {
                    int sec = (int)Math.Ceiling(_remaining.TotalSeconds);
                    SetTimerForeground(sec % 2 == 0 ? BlinkRedBrush : Brushes.White);
                }
            }
            else
            {
                _sw.Stop();
                _timer.Stop();
                _isRunning = false;
                _remaining = TimeSpan.Zero;
                UpdateDisplay();
                _countdownStarted = false;
                SetTimerForeground(Brushes.Black);
                _blinkState = false;
                _blinkTimer.Start();
                AnimateOverlayColor(RedOverlayBrush.Color);
                StatusLabel.Foreground = Brushes.Black;
                ShowStatusLabel();
                StartPauseButton.Content = "▶  Start";
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
            Overlay.Background = _overlayBrush;
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
            MinutesDisplay.Foreground = brush;
            SeparatorDisplay.Foreground = brush;
            SecondsDisplay.Foreground = brush;
        }

        private void ClampToMax60_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = (TextBox)sender;
            if (int.TryParse(tb.Text, out int val) && val > 60)
            {
                tb.Text = "60";
            }
        }

        private void UpdateDisplay()
        {
            MinutesDisplay.Text = ((int)_remaining.TotalMinutes).ToString("D2");
            SecondsDisplay.Text = _remaining.Seconds.ToString("D2");
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
            }
            else
            {
                if (_remaining.TotalSeconds <= 0)
                {
                    ResetToSelected();
                }
                if (_remaining.TotalSeconds <= 0) return;

                AnimateOverlayColor(NormalOverlayBrush.Color);
                StatusLabel.Visibility = Visibility.Collapsed;

                if (_remaining.TotalSeconds <= 10)
                {
                    StartCountdownEffects();
                }

                _remainingAtStart = _remaining;
                _sw.Reset();
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
            if (int.TryParse(_presetTimeBoxes[_selectedPresetIndex].Text, out int sec) && sec > 0)
                _selectedTime = TimeSpan.FromSeconds(sec);
            ResetToSelected();
            StartPauseButton.Content = "▶  Start";
        }

        private void PerformRetry()
        {
            _sw.Stop();
            _sw.Reset();
            _timer.Stop();
            _isRunning = false;
            if (int.TryParse(_presetTimeBoxes[_selectedPresetIndex].Text, out int sec) && sec > 0)
                _selectedTime = TimeSpan.FromSeconds(sec);
            ResetToSelected();
            StartPauseButton.Content = "▶  Start";
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && !_isClosing)
            {
                _isClosing = true;
                _sw.Stop();
                _timer.Stop();
                StopCountdownEffects();
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
                    PerformRetry();
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.T && !_isClosing)
            {
                if (!(Keyboard.FocusedElement is TextBox))
                {
                    PresetsPanel.Visibility = PresetsPanel.Visibility == Visibility.Visible
                        ? Visibility.Collapsed
                        : Visibility.Visible;
                    e.Handled = true;
                }
            }
        }
    }
}
