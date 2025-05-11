using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using BlagodatUstal.Models; // Убедитесь, что у вас есть правильный namespace для моделей

namespace BlagodatUstal
{
    public partial class MainWindow : Window
    {
        private int _attempts = 0;
        private bool _captchaRequired = false;
        private string _generatedCaptcha = string.Empty;
        private bool _passwordVisible = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void AuthorizeButtonClick(object? sender, RoutedEventArgs e)
        {
            var login = LoginTextBox.Text;
            var password = PasswordTextBox.Text;

            // Проверка CAPTCHA если требуется
            if (_captchaRequired && CaptchaTextBox.Text != _generatedCaptcha)
            {
                ErrorMessage.Text = "Неверная CAPTCHA";
                ErrorMessage.IsVisible = true;
                return;
            }

            using (var db = new User15Context()) // Замените YourDbContext на ваш реальный DbContext
            {
                var user = await db.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Username == login && u.Password == password);

                if (user != null)
                {
                    // Успешная авторизация
                    await SaveLoginHistory(db, user.UserId, true);

                    // Скрываем сообщение об ошибке
                    ErrorMessage.IsVisible = false;

                    // Открываем соответствующее окно
                    OpenUserWindow(user);
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
                            // Блокировка на 10 секунд
                            await BlockLoginForTime(10);
                        }
                    }
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
                IpAddress = "127.0.0.1" // Здесь можно добавить реальный IP
            };

            db.LoginHistories.Add(loginHistory);
            await db.SaveChangesAsync();
        }

        private void OpenUserWindow(User user)
        {
            Window userWindow;

            switch (user.Role.Name)
            {
                case "Администратор":
                    userWindow = new AdminWindow(user);
                    break;
                case "Старший смены":
                case "Продавец":
                    userWindow = new SellerWindow(user);
                    break;
                default:
                    throw new InvalidOperationException("Неизвестная роль пользователя");
            }

            userWindow.Show();
            this.Close();
        }

        private async Task BlockLoginForTime(int seconds)
        {
            LoginTextBox.IsEnabled = false;
            PasswordTextBox.IsEnabled = false;
            AuthorizeButton.IsEnabled = false;

            var endTime = DateTime.Now.AddSeconds(seconds);
            while (DateTime.Now < endTime)
            {
                var remaining = (endTime - DateTime.Now).Seconds;
                ErrorMessage.Text = $"Система заблокирована. Попробуйте через {remaining} секунд";
                await Task.Delay(1000);
            }

            LoginTextBox.IsEnabled = true;
            PasswordTextBox.IsEnabled = true;
            AuthorizeButton.IsEnabled = true;
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
            var symbols = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            _generatedCaptcha = new string(Enumerable.Range(0, 3)
                .Select(_ => symbols[random.Next(symbols.Length)])
                .ToArray());

            // TODO: Реализовать генерацию графической CAPTCHA с шумом
            // Пока просто отображаем текст для тестирования
            CaptchaTextBox.Watermark = $"Введите: {_generatedCaptcha}";
        }
    }
}