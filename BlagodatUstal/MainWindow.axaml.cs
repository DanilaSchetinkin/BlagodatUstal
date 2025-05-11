using Avalonia.Controls;
using System;
using System.Linq;
using Avalonia.Interactivity;

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

        private void AuthorizeButtonClick(object? sender, RoutedEventArgs e)
        {
            var login = LoginTextBox.Text;
            var password = PasswordTextBox.Text;

            if (_captchaRequired && CaptchaTextBox.Text != _generatedCaptcha)
            {
                ErrorMessage.Text = "�������� CAPTCHA";
                ErrorMessage.IsVisible = true;
                return;
            }

            if (login == "admin" && password == "admin")
            {
                ErrorMessage.IsVisible = false;
                // TODO: ������� �� ���� � ����������� �� ����
            }
            else
            {
                _attempts++;
                ErrorMessage.Text = "�������� ����� ��� ������";
                ErrorMessage.IsVisible = true;

                if (_attempts >= 2)
                {
                    _captchaRequired = true;
                    CaptchaPanel.IsVisible = true;
                    GenerateCaptcha();
                }
            }
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
            _generatedCaptcha = new string(Enumerable.Range(0, 3)
                .Select(_ => symbols[new Random().Next(symbols.Length)])
                .ToArray());

            // ����� ����� ������������� �������� � Image, �� ���� ����� ��� ��������
            CaptchaTextBox.Watermark = _generatedCaptcha;
        }
    }
}