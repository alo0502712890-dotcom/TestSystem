using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSystem.Models
{
    public class UserAnswer
    {
        public int UserAnswerID { get; set; }
        public int SessionID { get; set; }
        public int QuestionID { get; set; }
        public int AnswerID { get; set; }
        public DateTime AnsweredAt { get; set; }

        public TestSession Session { get; set; }
        public Question Question { get; set; }
        public Answer Answer { get; set; }
    }
}
