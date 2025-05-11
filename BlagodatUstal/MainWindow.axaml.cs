using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using BlagodatUstal.Models; // ���������, ��� � ��� ���� ���������� namespace ��� �������

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

            // �������� CAPTCHA ���� ���������
            if (_captchaRequired && CaptchaTextBox.Text != _generatedCaptcha)
            {
                ErrorMessage.Text = "�������� CAPTCHA";
                ErrorMessage.IsVisible = true;
                return;
            }

            using (var db = new User15Context()) // �������� YourDbContext �� ��� �������� DbContext
            {
                var user = await db.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Username == login && u.Password == password);

                if (user != null)
                {
                    // �������� �����������
                    await SaveLoginHistory(db, user.UserId, true);

                    // �������� ��������� �� ������
                    ErrorMessage.IsVisible = false;

                    // ��������� ��������������� ����
                    OpenUserWindow(user);
                }
                else
                {
                    _attempts++;
                    await SaveLoginHistory(db, null, false);

                    ErrorMessage.Text = "�������� ����� ��� ������";
                    ErrorMessage.IsVisible = true;

                    if (_attempts >= 2)
                    {
                        _captchaRequired = true;
                        CaptchaPanel.IsVisible = true;
                        GenerateCaptcha();

                        if (_attempts >= 3)
                        {
                            // ���������� �� 10 ������
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
                IpAddress = "127.0.0.1" // ����� ����� �������� �������� IP
            };

            db.LoginHistories.Add(loginHistory);
            await db.SaveChangesAsync();
        }

        private void OpenUserWindow(User user)
        {
            Window userWindow;

            switch (user.Role.Name)
            {
                case "�������������":
                    userWindow = new AdminWindow(user);
                    break;
                case "������� �����":
                case "��������":
                    userWindow = new SellerWindow(user);
                    break;
                default:
                    throw new InvalidOperationException("����������� ���� ������������");
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
                ErrorMessage.Text = $"������� �������������. ���������� ����� {remaining} ������";
                await Task.Delay(1000);
            }

            LoginTextBox.IsEnabled = true;
            PasswordTextBox.IsEnabled = true;
            AuthorizeButton.IsEnabled = true;
            ErrorMessage.Text = "���������� �����";
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

            // TODO: ����������� ��������� ����������� CAPTCHA � �����
            // ���� ������ ���������� ����� ��� ������������
            CaptchaTextBox.Watermark = $"�������: {_generatedCaptcha}";
        }
    }
}