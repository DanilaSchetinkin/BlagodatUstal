using Avalonia.Controls;
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

        public SellerWindow(User user)
        {
            InitializeComponent();
            _user = user;
            _sessionStart = DateTime.Now;
            InitializeSessionTimer();
        }

        private void InitializeSessionTimer()
        {
            _sessionTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1) // Проверка каждую минуту
            };
            _sessionTimer.Tick += CheckSessionTime;
            _sessionTimer.Start();
        }

        private void CheckSessionTime(object? sender, EventArgs e)
        {
            var elapsed = DateTime.Now - _sessionStart;
            if (elapsed.TotalMinutes >= 150) // 2.5 часа
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
                    UsernameAttempt = "Автоматический выход по таймауту"
                });
                db.SaveChanges();
            }
        }

        private string GetIpAddress()
        {
            try { return Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString(); }
            catch { return "unknown"; }
        }
    }