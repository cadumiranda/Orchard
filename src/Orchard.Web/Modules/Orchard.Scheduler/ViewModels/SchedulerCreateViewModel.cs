using System.ComponentModel.DataAnnotations;
using Orchard.Core.Common.ViewModels;
using Orchard.Scheduler.Models;

namespace Orchard.Scheduler.ViewModels {   
    public class SchedulerCreateViewModel {
        public SchedulerCreateViewModel() {
            Start = new DateTimeEditor { ShowDate = true, ShowTime = true };
            End = new DateTimeEditor { ShowDate = true, ShowTime = true };
            Form = new ExpressionForm();
        }

        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public DateTimeEditor Start { get; set; }
        
        public DateTimeEditor End { get; set; }
        
        public bool Enabled { get; set; }
        
        public ExpressionForm Form { get; set; }

        public ExpressionType ExpressionType { get; set; }

        public string Signals { get; set; }
    }    
}