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
                // Загружаем историю без включения пользователя
                var history = await db.LoginHistories
                    .OrderByDescending(lh => lh.LoginTime)
                    .ToListAsync();

                // Если нужно загрузить пользователей отдельно
                foreach (var item in history.Where(lh => lh.UserId.HasValue))
                {
                    item.User = await db.Users.FindAsync(item.UserId.Value);
                }

                HistoryGrid.ItemsSource = history;
            }
        }
    }
}