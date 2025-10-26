using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSystem.Models
{
    public class Question
    {
        public int QuestionID { get; set; }
        public int TestID { get; set; }
        public string QuestionText { get; set; }
        public string QuestionType { get; set; } // Single / Multiple
        public int Weight { get; set; }
        public string ImagePath { get; set; }
        public int SortOrder { get; set; }

        public Test Test { get; set; }
        public ICollection<Answer> Answers { get; set; }
        public ICollection<UserAnswer> UserAnswers { get; set; }
    }
}
