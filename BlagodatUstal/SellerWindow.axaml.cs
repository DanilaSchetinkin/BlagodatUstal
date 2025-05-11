using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using BlagodatUstal.Models;
using System;
using System.Net;

namespace BlagodatUstal
{
    public partial class SellerWindow : Window  
    {
        private readonly User _user;
        private DispatcherTimer _sessionTimer;
        private DateTime _sessionStart;
        private TimeSpan _sessionDuration;

        public SellerWindow(User user)
        {
            InitializeComponent();
            _user = user;
            _sessionStart = DateTime.Now;
            InitializeSessionTimer();
            UpdateTimerDisplay();
        }

        private void UpdateTimerDisplay()
        {
            // ��������� ����������� �������
            if (SessionTimerText != null)
            {
                SessionTimerText.Text = $"����� ������: {_sessionDuration:hh\\:mm\\:ss}";
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            // �������� ����� Logout() �� ���������������� ���
            _sessionTimer?.Stop();
            LogSessionEnd();
            Close();
            new MainWindow().Show();
        }

        private void InitializeSessionTimer()
        {
            _sessionTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1) // �������� �� 1 ������� ��� �������� ����������
            };
            _sessionTimer.Tick += (s, e) =>
            {
                _sessionDuration = DateTime.Now - _sessionStart;
                UpdateTimerDisplay();

                if (_sessionDuration.TotalMinutes >= 150) // 2.5 ����
                {
                    _sessionTimer.Stop();
                    LogSessionEnd();
                    Close();
                    new MainWindow().Show();
                }
            };
            _sessionTimer.Start();
        }

        private void CheckSessionTime(object? sender, EventArgs e)
        {
            var elapsed = DateTime.Now - _sessionStart;
            if (elapsed.TotalMinutes >= 150) // 2.5 ����
            {
                LogSessionEnd();
                Close();
                new MainWindow().Show();
            }
        }

        private void LogSessionEnd()
        {
            using (var db = new User15Context())
            {
                db.LoginHistories.Add(new LoginHistory
                {
                    UserId = _user.Id,
                    LoginTime = DateTime.Now,
                    IsSuccess = false,
                    IpAddress = GetIpAddress(),
                    Description = "������ ���������" // ���������� Description ������ UsernameAttempt
                });
                db.SaveChanges();
            }
        }

        private string GetIpAddress()
        {
            try
            {
                return Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString();
            }
            catch
            {
                return "unknown";
            }
        }
    }
}