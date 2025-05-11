using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlagodatUstal.Models;
using Avalonia.Threading;

namespace BlagodatUstal
{

    

    public partial class MainWindow : Window
    {
        private int _attempts = 0;
        private bool _captchaRequired = false;
        private string _generatedCaptcha = string.Empty;
        private bool _passwordVisible = false;
        private DispatcherTimer _sessionTimer;
        private int _sessionTime = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        public class UserWithRole
        {
            public User User { get; set; }
            public Role Role { get; set; }
        }

        private async void AuthorizeButtonClick(object? sender, RoutedEventArgs e)
        {
            var login = LoginTextBox.Text;
            var password = PasswordTextBox.Text;

            if (_captchaRequired && CaptchaTextBox.Text != _generatedCaptcha)
            {
                ErrorMessage.Text = "Неверная CAPTCHA";
                ErrorMessage.IsVisible = true;
                return;
            }

            using (var db = new User15Context())
            {
                try
                {
                    var userWithRole = await db.Users
                        .Where(u => u.Email == login && u.Password == password)
                        .Join(db.Roles,
                            user => user.RoleId,
                            role => role.RoleId,
                            (user, role) => new UserWithRole { User = user, Role = role })
                        .FirstOrDefaultAsync();

                    if (userWithRole != null)
                    {
                        await SaveLoginHistory(db, userWithRole.User.Id, true);
                        ErrorMessage.IsVisible = false;
                        OpenUserWindow(userWithRole.User, userWithRole.Role);
                    }
                    else
                    {
                        _attempts++;
                        await SaveLoginHistory(db, null, false);
                        ErrorMessage.Text = "Неверный логин или пароль";
                        ErrorMessage.IsVisible = true;

                        if (_attempts >= 2)
                        {
                            _captchaRequired = true;
                            CaptchaPanel.IsVisible = true;
                            GenerateCaptcha();

                            if (_attempts >= 3)
                            {
                                await BlockLoginForTime(10);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage.Text = $"Ошибка: {ex.Message}";
                    ErrorMessage.IsVisible = true;
                }
            }
        }

        private async Task SaveLoginHistory(User15Context db, int? userId, bool isSuccess)
        {
            var loginHistory = new LoginHistory
            {
                UserId = userId,
                LoginTime = DateTime.Now,
                IsSuccess = isSuccess,
                IpAddress = "127.0.0.1"
            };
            db.LoginHistories.Add(loginHistory);
            await db.SaveChangesAsync();
        }

        private void OpenUserWindow(User user, Role role)
        {
            if (role?.RoleName == null)
            {
                throw new InvalidOperationException("Роль пользователя не определена");
            }

            Window userWindow = role.RoleName switch
            {
                "Администратор" => new AdminWindow(user),
                "Старший смены" or "Продавец" => new SellerWindow(user),
                _ => throw new InvalidOperationException($"Неизвестная роль пользователя: {role.RoleName}")
            };

            userWindow.Show();
            this.Close();
        }

        private async Task BlockLoginForTime(int seconds)
        {
            LoginTextBox.IsEnabled = false;
            PasswordTextBox.IsEnabled = false;
            var authorizeButton = this.FindControl<Button>("AuthorizeButton");
            authorizeButton.IsEnabled = false;

            for (int i = seconds; i > 0; i--)
            {
                ErrorMessage.Text = $"Система заблокирована. Попробуйте через {i} секунд";
                await Task.Delay(1000);
            }

            LoginTextBox.IsEnabled = true;
            PasswordTextBox.IsEnabled = true;
            authorizeButton.IsEnabled = true;
            ErrorMessage.Text = "Попробуйте снова";
        }

        private void TogglePasswordVisibility(object? sender, RoutedEventArgs e)
        {
            _passwordVisible = !_passwordVisible;
            PasswordTextBox.PasswordChar = _passwordVisible ? '\0' : '*';
        }

        private void RefreshCaptcha(object? sender, RoutedEventArgs e)
        {
            GenerateCaptcha();
        }

        private void GenerateCaptcha()
        {
            var random = new Random();
            var symbols = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            _generatedCaptcha = new string(Enumerable.Repeat(symbols, 3)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            var width = 150;
            var height = 50;
            var bitmap = new RenderTargetBitmap(new PixelSize(width, height));

            using (var ctx = bitmap.CreateDrawingContext())
            {
               
            }

            CaptchaImage.Source = bitmap;
        }

        private void StartSessionTimer()
        {
            _sessionTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _sessionTimer.Tick += (s, e) => {
                _sessionTime++;
                if (_sessionTime >= 600) ForceLogout(); // 10 минут
                else if (_sessionTime >= 300) ShowWarning(); // 5 минут
            };
            _sessionTimer.Start();
        }

        private void ShowWarning()
        {
            // Реализация предупреждения
        }

        private void ForceLogout()
        {
            // Реализация выхода
        }
    }
}