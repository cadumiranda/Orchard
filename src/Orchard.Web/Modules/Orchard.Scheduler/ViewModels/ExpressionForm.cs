using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Orchard.Scheduler.ViewModels {
    public class ExpressionForm {
        public ExpressionForm() {
            Days = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>().Select(x =>
            {
                return new SelectListItem { Text = x.ToString().Substring(0, 3).ToUpper(), Value = ((int)x).ToString() };
            }).ToList();
        }

        public ExpressionForm(string[] expression)
            : this() {
            Minute = expression[0];
            Hour = expression[1];
            DayOfMonth = expression[2];
            Month = expression[3];
            SetSelectedDays(expression[4]);
        }

        public IList<SelectListItem> Days { get; set; }
        public string Month { get; set; }
        public string DayOfMonth { get; set; }
        public string Hour { get; set; }
        public string Minute { get; set; }

        private string dayOfWeek;
        public string DayOfWeek {
            set {
                dayOfWeek = value;
            }
            get {
                if (!string.IsNullOrEmpty(dayOfWeek)) return dayOfWeek;
                return string.Join(",", Days.Where(x => x.Selected).Select(x => x.Value).ToArray());
            }
        }

        private void SetSelectedDays(string dayOfWeek) {
            if (dayOfWeek.Contains(',') && dayOfWeek.Contains('-')) {
                var values = dayOfWeek.Split(',');
                foreach (var v in values) {
                    if (v.Contains('-')) {
                        SetDaysFromRange(v.Split('-'));
                    }
                    else {
                        var day = Days.FirstOrDefault(x => x.Value == v);
                        if (day != null) {
                            day.Selected = true;
                        }
                    }
                }
            }
            else if (dayOfWeek.Contains(',') || dayOfWeek.Contains('-')) {
                var values = dayOfWeek.Split(',', '-');
                SetDaysFromRange(values);
            }
            else {
                this.dayOfWeek = dayOfWeek;
            }
        }

        private void SetDaysFromRange(string[] values) {
            var range = Enumerable.Range(int.Parse(values[0]), int.Parse(values[values.Length - 1]) + 1);
            foreach (var day in Days) {
                if (range.Contains(int.Parse(day.Value))) {
                    day.Selected = true;
                }
            }
        }

        public string GetExpression() {
            return string.Join(" ", new[] { Minute, Hour, DayOfMonth, Month, DayOfWeek }.Select(x => string.IsNullOrEmpty(x) ? "*" : x));
        }
    }
}