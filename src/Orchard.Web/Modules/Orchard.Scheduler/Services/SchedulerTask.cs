using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Orchard.Tasks;
using Orchard.Scheduler.Models;
using Orchard.Data;
using Orchard.Services;
using NCrontab;
using Orchard.Logging;
using Orchard.Reports.Services;
using Orchard.TaskLease.Services;
using Orchard.Workflows.Activities;
using Orchard.Workflows.Services;


namespace Orchard.Scheduler.Services {
    public class SchedulerTask : Component, IBackgroundTask {
        private readonly IRepository<ScheduleRecord> _repository;
        private static readonly object _sweepLock = new object();
        private readonly ITaskLeaseService _taskLeaseService;
        private readonly IClock _clock;
        private readonly IWorkflowManager _workflowManager;
        private readonly IReportsCoordinator _reportsCoordinator;

        public SchedulerTask(IRepository<ScheduleRecord> repository, 
            IClock clock,
            IWorkflowManager workflowManager,
            ITaskLeaseService taskLeaseService,
            IReportsCoordinator reportsCoordinator) {
            _repository = repository;            
            _clock = clock;
            _workflowManager = workflowManager;
            _taskLeaseService = taskLeaseService;
            _reportsCoordinator = reportsCoordinator;
        }

        public void Sweep() {
           if (Monitor.TryEnter(_sweepLock)) {
                try {
                    Logger.Debug("Beginning SchedulerTask sweep.");

                    // Only allow this task to run on one farm node at a time.
                    if (_taskLeaseService.Acquire(GetType().FullName, _clock.UtcNow.AddHours(1)) != null) {

                        var now = _clock.UtcNow;
                        var records = _repository.Fetch(x => x.Enabled && x.StartDateUtc <= now && x.NextOccurrenceUtc <= now);
                        Logger.Debug("Orchard.Scheduler.SchedulerTask found {0} rows at {1}", records.Count(), now);

                        foreach (var record in records) {
                            try {
                                Logger.Debug("Orchard.Scheduler.SchedulerTask about to trigger schedule '{0}'", record.Name);
                                var signals = record.Signals.Split(',').Select(s => s.Trim());
                                foreach (var signal in signals) {
                                    _workflowManager.TriggerEvent(SignalActivity.SignalEventName, null, () => new Dictionary<string, object> {{SignalActivity.SignalEventName, signal}});
                                }
                                _reportsCoordinator.Information("Scheduler", string.Format("Schedule event '{0}' triggered successfully", record.Name));

                                var cronTab = CrontabSchedule.Parse(record.CronExpression);
                                var nextOccurrence = record.EndDateUtc.HasValue ? cronTab.GetNextOccurrence(now, record.EndDateUtc.Value) : cronTab.GetNextOccurrence(now);

                                if (nextOccurrence != DateTime.MinValue) {
                                    record.NextOccurrenceUtc = nextOccurrence.ToUniversalTime();
                                    _repository.Update(record);
                                }

                                if (record.EndDateUtc.HasValue && record.EndDateUtc.Value <= now) {
                                    record.Enabled = false;
                                    _repository.Update(record);
                                }
                            } catch (Exception ex) {
                                _reportsCoordinator.Error("Scheduler", string.Format("Error triggering '{0}': {1}", record.Name, ex.Message));
                                Logger.Error(ex, "Exception occurred during triggering of schedule '{0}'", record.Name);
                            }
                        }
                    }
                } catch (Exception ex) {
                    Logger.Error(ex, "Error during SchedulerTask sweep.");
                } finally {
                    Monitor.Exit(_sweepLock);
                    Logger.Debug("Ending SchedulerTask sweep.");
                }
            }
        }
    }
}