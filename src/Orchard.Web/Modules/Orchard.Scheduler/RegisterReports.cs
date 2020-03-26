using Orchard.Environment;
using Orchard.Environment.Extensions.Models;
using Orchard.Reports.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.Scheduler
{
    public class RegisterReports : IFeatureEventHandler
    {
        private readonly IReportsCoordinator _reportsCoordinator;

        public RegisterReports(IReportsCoordinator reportsCoordinator)
        {
            _reportsCoordinator = reportsCoordinator;
        }

        public void Disabled(Feature feature)
        {
        }

        public void Disabling(Feature feature)
        {
        }

        public void Enabled(Feature feature)
        {
        }

        public void Enabling(Feature feature)
        {
        }

        public void Installed(Feature feature)
        {
            //_reportsCoordinator.Register("Scheduler", "Schedule Triggered", "via Rules API");
            //_reportsCoordinator.Register("IScheduledTask.Execute", "Schedule Triggered", "via IScheduledTask interface");
        }

        public void Installing(Feature feature)
        {
        }

        public void Uninstalled(Feature feature)
        {
        }

        public void Uninstalling(Feature feature)
        {
        }
    }
}