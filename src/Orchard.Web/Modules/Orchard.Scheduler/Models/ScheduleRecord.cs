using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.Scheduler.Models {
    public enum ExpressionType {
        Basic,
        Advanced
    }

    public class ScheduleRecord {
        public virtual int Id { get; set; }

        public virtual string Name { get; set; }

        public virtual bool Enabled { get; set; }

        public virtual DateTime StartDateUtc { get; set; }

        public virtual DateTime? EndDateUtc { get; set; }

        public virtual string CronExpression { get; set; }

        public virtual DateTime? NextOccurrenceUtc { get; set; }

        public virtual ExpressionType Type { get; set; }

        public virtual string Signals { get; set; }
    }
}