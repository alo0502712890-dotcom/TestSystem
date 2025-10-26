using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSystem.Models
{
    public class TestSession
    {
        [Key]
        public int SessionID { get; set; }
        public int UserID { get; set; }
        public int TestID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal? Score { get; set; }
        public decimal? MaxScore { get; set; }
        public bool IsCompleted { get; set; }

        public User User { get; set; }
        public Test Test { get; set; }
        public ICollection<UserAnswer> UserAnswers { get; set; }
    }
}
