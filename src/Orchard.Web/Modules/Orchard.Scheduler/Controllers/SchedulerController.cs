using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.Core.Common.ViewModels;
using Orchard.Localization.Services;
using Orchard.UI.Admin;
using Orchard.Security;
using Orchard.Localization;
using Orchard.Scheduler.ViewModels;
using Orchard.UI.Navigation;
using Orchard.Settings;
using Orchard.Scheduler.Models;
using Orchard.Data;
using System.Web.Routing;
using Orchard.ContentManagement;
using NCrontab;
using Orchard.UI.Notify;

namespace Orchard.Scheduler.Controllers {
    public class SchedulerController : Controller, IUpdateModel {
        private readonly IOrchardServices _services;
        private readonly ISiteService _siteService;
        private readonly IRepository<ScheduleRecord> _repository;
        private readonly IDateLocalizationServices _dateLocalizationServices;

        public SchedulerController(IOrchardServices services,
            ISiteService siteService,
            IRepository<ScheduleRecord> repository,
            IDateLocalizationServices dateLocalizationServices) {
            _services = services;
            _siteService = siteService;
            _repository = repository;
            _dateLocalizationServices = dateLocalizationServices;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }   

        [Admin]
        public ActionResult Index(SchedulerIndexOptions options, PagerParameters pagerParameters) {
            if (!_services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage Schedules")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            options = options ?? new SchedulerIndexOptions();

            var schedules = _repository.Table;

            switch (options.Filter) {
                case SchedulerFilter.Disabled:
                    schedules = schedules.Where(r => r.Enabled == false);
                    break;
                case SchedulerFilter.Enabled:
                    schedules = schedules.Where(u => u.Enabled);
                    break;
            }

            if (!string.IsNullOrWhiteSpace(options.Search)) {
                schedules = schedules.Where(r => r.Name.Contains(options.Search));
            }

            switch (options.Order) {
                case SchedulerOrder.Name:
                    schedules = schedules.OrderBy(u => u.Name);
                    break;
                case SchedulerOrder.StartDateUtc:
                    schedules = schedules.OrderBy(u => u.StartDateUtc);
                    break;
                case SchedulerOrder.EndDateUtc:
                    schedules = schedules.OrderBy(u => u.EndDateUtc);
                    break;
                case SchedulerOrder.NextOccurrenceUtc:
                    schedules = schedules.OrderBy(u => u.NextOccurrenceUtc);
                    break;
            }

            var results = schedules
                .Skip(pager.GetStartIndex())
                .Take(pager.PageSize)
                .ToList();

            var pagerShape = _services.New.Pager(pager).TotalItemCount(schedules.Count());

            var model = new SchedulerIndexViewModel {
                Schedules = results.Select(x => new ScheduleEntry
                {
                    Record = x,
                    IsChecked = false
                }).ToList(),
                Options = options,
                Pager = pagerShape
            };

            // maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Filter", options.Filter);
            routeData.Values.Add("Options.Search", options.Search);
            routeData.Values.Add("Options.Order", options.Order);

            pagerShape.RouteData(routeData);

            return View(model);
        }

        [Admin]
        public ActionResult Create() {
            if (!_services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage Schedules")))
                return new HttpUnauthorizedResult();

            return View(new SchedulerCreateViewModel());
        }

        [Admin]
        [HttpPost]
        [ActionName("Create")]        
        public ActionResult CreatePOST(SchedulerCreateViewModel model) {
            if (!_services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage Schedules")))
                return new HttpUnauthorizedResult();

            UpdateModel(model.Form);
            ValidateDates(model);

            if (!ModelState.IsValid)
                return View(model);

            var expression = model.Form.GetExpression(); ;            
            var cronTab = CrontabSchedule.TryParse(expression);

            if (cronTab.IsError) {
                _services.Notifier.Error(T("Invalid cron expression, please try again"));
                _services.Notifier.Error(T(cronTab.Error.Message));
                return View(model);
            }

            var record = new ScheduleRecord();
            Map(model, record, cronTab.Value);
            _repository.Create(record);

            _services.Notifier.Information(T("Schedule '{0}' created successfully", record.Name));
            return RedirectToAction("Index");            
        }        

        [HttpPost]
        public ActionResult NextOccurrences(ExpressionForm model) {
            UpdateModel(model);
            var expression = string.Join(" ", new[] { model.Minute, model.Hour, model.DayOfMonth, model.Month, model.DayOfWeek }.Select(x => string.IsNullOrEmpty(x) ? "*" : x));
            var cronTab = CrontabSchedule.TryParse(expression);

            var result = new NextOccurencesViewModel();
            if (cronTab.IsError) {
                result.Error = cronTab.Error.Message;
            }
            else {
                result.CronExpression = cronTab.Value.ToString();
                result.NextOccurrences = cronTab.Value.GetNextOccurrences(DateTime.Now, DateTime.Now.AddYears(2));
            }

            return PartialView("_NextOccurrences", result);
        }

        [Admin]
        public ActionResult Edit(int id) {
            if (!_services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage Schedules")))
                return new HttpUnauthorizedResult();

            var record = _repository.Get(id);
            if (record == null)
                return HttpNotFound();

            var model = new SchedulerCreateViewModel();            
            model.Id = record.Id;
            model.Name = record.Name;
            model.Enabled = record.Enabled;
            model.Start = new DateTimeEditor {
                ShowDate = true,
                ShowTime = true,
                Date = _dateLocalizationServices.ConvertToLocalizedDateString(record.StartDateUtc),
                Time = _dateLocalizationServices.ConvertToLocalizedTimeString(record.StartDateUtc)
            };
            model.End = new DateTimeEditor {
                ShowDate = true,
                ShowTime = true,
                Date = record.EndDateUtc.HasValue ? _dateLocalizationServices.ConvertToLocalizedDateString(record.EndDateUtc.Value) : "",
                Time = record.EndDateUtc.HasValue ? _dateLocalizationServices.ConvertToLocalizedTimeString(record.EndDateUtc.Value) : "",
            };
            model.ExpressionType = record.Type;

            var expression = record.CronExpression.Split(' ');
            model.Form = new ExpressionForm(expression);
            model.Signals = record.Signals;

            return View("Create", model);
        }

        [Admin]
        [HttpPost]
        [ActionName("Edit")]
        public ActionResult EditPOST(SchedulerCreateViewModel model) {
            if (!_services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage Schedules")))
                return new HttpUnauthorizedResult();

            UpdateModel(model.Form);
            ValidateDates(model);
            
            if (!ModelState.IsValid)
                return View("Create", model);

            var record = _repository.Get(model.Id);
            if (record == null)
                return HttpNotFound();

            var expression = model.Form.GetExpression();
            var cronTab = CrontabSchedule.TryParse(expression);

            if (cronTab.IsError) {
                _services.Notifier.Error(T("Invalid cron expression, please try again"));
                _services.Notifier.Error(T(cronTab.Error.Message));
                return View("Create", model);
            }

            Map(model, record, cronTab.Value);
            _repository.Update(record);

            _services.Notifier.Information(T("Schedule '{0}' updated successfully", record.Name));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id) {
            if (!_services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage Schedules")))
                return new HttpUnauthorizedResult();

            var record = _repository.Get(id);

            if (record != null) {
                _repository.Delete(record);
                _services.Notifier.Information(T("Schedule {0} deleted", record.Name));
            }

            return RedirectToAction("Index");
        }

        public ActionResult Enable(int id) {
            if (!_services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage Schedules")))
                return new HttpUnauthorizedResult();

            var record = _repository.Get(id);

            if (record != null) {
                record.Enabled = true;
                _services.Notifier.Information(T("Schedule '{0}' enabled", record.Name));
            }

            return RedirectToAction("Index");
        }

        public ActionResult Disable(int id) {
            if (!_services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage Schedules")))
                return new HttpUnauthorizedResult();

            var record = _repository.Get(id);

            if (record != null) {
                record.Enabled = false;
                _services.Notifier.Information(T("Schedule '{0}' disabled", record.Name));
            }

            return RedirectToAction("Index");
        }

        public ActionResult ExpressionForm(ExpressionType type, int id) {
            ExpressionForm form;
            if (id > 0) {
                var record = _repository.Get(id);
                var expression = record.CronExpression.Split(' ');
                form = new ExpressionForm(expression);
            }
            else {
                form = new ExpressionForm();
            }            
            switch (type) {
                case ExpressionType.Basic:
                    return PartialView("_BasicExpression", form);
                case ExpressionType.Advanced:
                    return PartialView("_AdvancedExpression", form);
            }
            return new EmptyResult();
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        public void AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        private void Map(SchedulerCreateViewModel model, ScheduleRecord record, CrontabSchedule cronTab) {
            record.Name = model.Name;
            record.Enabled = model.Enabled;
            record.Type = model.ExpressionType;
            record.StartDateUtc = _dateLocalizationServices.ConvertFromLocalizedString(model.Start.Date, model.Start.Time).Value;
            record.EndDateUtc = _dateLocalizationServices.ConvertFromLocalizedString(model.End.Date, model.End.Time);
            record.CronExpression = cronTab.ToString();
            record.NextOccurrenceUtc = GetNextOccurrence(cronTab, record.StartDateUtc);
            record.Signals = model.Signals;
        }

        private DateTime GetNextOccurrence(CrontabSchedule schedule, DateTime startDateUtc) {
            var nowUtc = DateTime.Now.ToUniversalTime();
            if (startDateUtc > nowUtc) {
                return schedule.GetNextOccurrence(startDateUtc).ToUniversalTime();
            }
            return schedule.GetNextOccurrence(nowUtc).ToUniversalTime();
        }

        private void ValidateDates(SchedulerCreateViewModel model) {
            if (!String.IsNullOrWhiteSpace(model.Start.Date)) {
                try {
                    if (string.IsNullOrEmpty(model.Start.Time)) model.Start.Time = "00:00";
                    var utcStartDateTime = _dateLocalizationServices.ConvertFromLocalizedString(model.Start.Date, model.Start.Time);
                    if (string.IsNullOrEmpty(model.End.Date)) model.End.Time = null;
                    else if (string.IsNullOrEmpty(model.End.Time)) model.End.Time = "00:00";
                    var utcEndDateTime = (!String.IsNullOrWhiteSpace(model.End.Date))
                                             ? _dateLocalizationServices.ConvertFromLocalizedString(model.End.Date, model.End.Time)
                                             : null;
                    if (utcEndDateTime.HasValue && utcStartDateTime >= utcEndDateTime.Value)
                        AddModelError("Start.Date", T("Start date must be before the End date of the Schedule"));
                } catch (FormatException) {
                    AddModelError("Start.Date", T("'{0} {1}' could not be parsed as a valid date and time.", model.Start.Date, model.Start.Time));
                }
            } else {
                AddModelError("Start.Date", T("Please enter a start date."));
            }
        }
    }
}