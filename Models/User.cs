using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace TestSystem.Models
{
    public partial class User
    {
        public int UserId { get; set; }
        public string Login { get; set; }
        public string PasswordHash { get; set; }
        public string? Salt { get; set; }
        public string UserType { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }

        public virtual ICollection<Test> Tests { get; set; } = new List<Test>();
        public virtual ICollection<TestSession> TestSessions { get; set; } = new List<TestSession>();
    }
}
