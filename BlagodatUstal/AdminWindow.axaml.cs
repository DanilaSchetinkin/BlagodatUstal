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

        private void LoadHistory()
        {
            using (var db = new User15Context())
            {
                HistoryGrid.Items = db.LoginHistories
                    .Include(lh => lh.User)
                    .OrderByDescending(lh => lh.LoginTime)
                    .ToList();
            }
        }
    }
}