using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSystem.Models
{
    public class Test
    {
        public int TestID { get; set; }
        public string TestName { get; set; }
        public string Description { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? TimeLimit { get; set; }
        public int MaxAttempts { get; set; }
        public bool IsActive { get; set; }


        [ForeignKey("CreatedBy")]
        public User Creator { get; set; }
        public ICollection<Question> Questions { get; set; }
        public ICollection<TestSession> TestSessions { get; set; }
    }
}
