using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSystem.Models
{
    public class Question
    {
        public int QuestionID { get; set; }
        public int TestID { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public int Weight { get; set; }
        public string? ImagePath { get; set; }
        public int SortOrder { get; set; }

        [ForeignKey("TestID")]
        public Test Test { get; set; }
        public ICollection<Answer> Answers { get; set; } = new List<Answer>();
        public ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
    }
}
