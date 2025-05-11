using Avalonia.Controls;
using BlagodatUstal.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BlagodatUstal
{
    public partial class AdminWindow : Window
    {
        public AdminWindow(User user)
        {
            InitializeComponent();
            LoadHistory();
        }

        private async void LoadHistory()
        {
            using (var db = new User15Context())
            {
                // ��������� ������� ��� ��������� ������������
                var history = await db.LoginHistories
                    .OrderByDescending(lh => lh.LoginTime)
                    .ToListAsync();

                // ���� ����� ��������� ������������� ��������
                foreach (var item in history.Where(lh => lh.UserId.HasValue))
                {
                    item.User = await db.Users.FindAsync(item.UserId.Value);
                }

                HistoryGrid.ItemsSource = history;
            }
        }
    }
}