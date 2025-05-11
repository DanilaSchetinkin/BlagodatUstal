using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace BlagodatUstal.Models
{
    public partial class LoginHistory
    {
        [Key]
        public int Id { get; set; }
        public int? UserId { get; set; }
        public DateTime LoginTime { get; set; }
        public bool IsSuccess { get; set; }
        public string IpAddress { get; set; }
        public string Description { get; set; } // Добавляем это свойство

        // Если нужно связать с пользователем
        public virtual User User { get; set; }
    }
}