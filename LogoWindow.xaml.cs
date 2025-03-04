using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace STITimer
{
    public partial class LogoWindow : Window
    {
        private bool _isClosing;

        public LogoWindow()
        {
            InitializeComponent();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && !_isClosing)
            {
                _isClosing = true;
                var fadeOut = new DoubleAnimation(1, 0, System.TimeSpan.FromSeconds(0.35))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
                };
                fadeOut.Completed += (s, _) => Close();
                BeginAnimation(OpacityProperty, fadeOut);
            }
        }
    }
}
