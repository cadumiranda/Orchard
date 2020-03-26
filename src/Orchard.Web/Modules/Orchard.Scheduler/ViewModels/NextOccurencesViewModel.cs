using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.Scheduler.ViewModels {
    public class NextOccurencesViewModel {
        public string Error { get; set; }
        public string CronExpression  { get; set; }
        public IEnumerable<DateTime> NextOccurrences { get; set; }
    }
}