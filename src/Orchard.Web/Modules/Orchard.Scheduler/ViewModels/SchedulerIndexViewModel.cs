using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Scheduler.Models;

namespace Orchard.Scheduler.ViewModels {
    public enum SchedulerOrder {
        Name,
        StartDateUtc,
        EndDateUtc,
        NextOccurrenceUtc
    }

    public enum SchedulerFilter {
        All,
        Enabled,
        Disabled
    }

    public enum SchedulerBulkAction {
        None,
        Enable,
        Disable,
        Delete
    }

    public class SchedulerIndexViewModel {
        public IList<ScheduleEntry> Schedules { get; set; }
        public SchedulerIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class ScheduleEntry {
        public ScheduleRecord Record { get; set; }
        public bool IsChecked { get; set; }
    }

    public class SchedulerIndexOptions {
        public string Search { get; set; }
        public SchedulerOrder Order { get; set; }
        public SchedulerFilter Filter { get; set; }
        public SchedulerBulkAction BulkAction { get; set; }
    }
}