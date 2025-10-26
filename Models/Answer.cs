using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSystem.Models
{
    public class Answer
    {
        public int AnswerID { get; set; }
        public int QuestionID { get; set; }
        public string AnswerText { get; set; }
        public bool IsCorrect { get; set; }
        public int SortOrder { get; set; }

        public Question Question { get; set; }
    }
}
