using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSystem.Models
{
    public class TestSessionInfo
    {
        public int SessionID { get; set; }
        public string TestName { get; set; } // <--- Це те, чого не вистачає в TestSession
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal? Score { get; set; }
        public decimal? MaxScore { get; set; }
        public bool IsCompleted { get; set; }


        // Допоміжні властивості для відображення
        public string CompletionDate => EndTime.HasValue ? EndTime.Value.ToString("dd.MM.yyyy HH:mm") : "Триває";
        public string DisplayScore => IsCompleted ? $"{Score}/{MaxScore}" : "N/A";
    }
}
