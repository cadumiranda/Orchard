using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NCrontab;

namespace Orchard.Scheduler.Services {
    public static class StandardSchedules {
        public static CrontabSchedule Hourly(int minute = 0) {
            if (minute < 0 || minute > 59)
                throw new ArgumentOutOfRangeException("minute");

            return CrontabSchedule.Parse(string.Format("{0} 0 * * *", minute));
        }

        public static CrontabSchedule Daily(int hour = 0){
            if (hour < 0 || hour > 23)
                throw new ArgumentOutOfRangeException("hour");

            return CrontabSchedule.Parse(string.Format("0 {0} * * *", hour));
        }

        public static CrontabSchedule Weekly(int day = 0) {
            if (day < 0 || day > 6)
                throw new ArgumentOutOfRangeException("day");

            return CrontabSchedule.Parse(string.Format("0 0 * * {0}", day));
        }

        public static CrontabSchedule DayOfMonth(int dayOfMonth = 0) {
            if (dayOfMonth < 0 || dayOfMonth > 31)
                throw new ArgumentOutOfRangeException("dayOfMonth");

            return CrontabSchedule.Parse(string.Format("0 0 {0} * *", dayOfMonth));
        }
    }
}