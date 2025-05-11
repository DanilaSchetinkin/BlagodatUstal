using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlagodatUstal.Models
{
    public class LoginHistory
    {
        public int LoginHistoryId { get; set; }
        public int? UserId { get; set; }
        public User User { get; set; }
        public DateTime LoginTime { get; set; }
        public bool IsSuccess { get; set; }
        public string IpAddress { get; set; }
    }
}