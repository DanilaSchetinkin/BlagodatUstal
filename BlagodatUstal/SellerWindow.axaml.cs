using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using BlagodatUstal;
using System;
using System.Timers;

namespace YourNamespace
{
    public partial class SellerWindow : Window
    {
        private Timer _sessionTimer;
        private DateTime _sessionStartTime;
        private TimeSpan _warningThreshold = TimeSpan.FromMinutes(5);
        private TimeSpan _logoutThreshold = TimeSpan.FromMinutes(10);

        public SellerWindow()
        {
            InitializeComponent();
            StartSessionTimer();
        }

        private void StartSessionTimer()
        {
            _sessionStartTime = DateTime.Now;

            _sessionTimer = new Timer(1000); // Обновление каждую секунду
            _sessionTimer.Elapsed += OnTimerElapsed;
            _sessionTimer.Start();
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            var elapsed = DateTime.Now - _sessionStartTime;

            Dispatcher.UIThread.Post(() =>
            {
                SessionTimerText.Text = $"Сеанс: {elapsed:mm\\:ss}";

                if (elapsed >= _warningThreshold && elapsed < _logoutThreshold)
                {
                    SessionTimerText.Foreground = Avalonia.Media.Brushes.Orange;
                }

                if (elapsed >= _logoutThreshold)
                {
                    _sessionTimer.Stop();
                    SessionTimerText.Foreground = Avalonia.Media.Brushes.Red;
                    SessionTimerText.Text = "Сеанс завершён";

                    MessageBox("Сессия завершена. Вы будете выведены из системы.");
                    Dispatcher.UIThread.Post(() =>
                    {
                        Close();
                        // Можно здесь показать снова MainWindow
                        new MainWindow().Show();
                    });
                }
            });
        }

        private void Logout_Click(object? sender, RoutedEventArgs e)
        {
            _sessionTimer?.Stop();
            Close();
            new MainWindow().Show();
        }

        private async void MessageBox(string message)
        {
            var dlg = new Window
            {
                Width = 300,
                Height = 150,
                Title = "Сообщение",
                Content = new TextBlock
                {
                    Text = message,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap
                }
            };

            await dlg.ShowDialog(this);
        }
    }
}