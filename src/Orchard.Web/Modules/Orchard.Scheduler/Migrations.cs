using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Data.Migration;

namespace Orchard.Scheduler {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("ScheduleRecord",
                table => table
                    .Column<int>("Id", c => c.PrimaryKey().Identity())
                    .Column<DateTime>("StartDateUtc")
                    .Column<DateTime>("EndDateUtc")
                    .Column<DateTime>("NextOccurrenceUtc")
                    .Column<string>("CronExpression", c => c.WithLength(255))
                    .Column<bool>("Enabled")
                    .Column<string>("Name", c => c.WithLength(1024))
                );

            return 1;
        }

        public int UpdateFrom1() {
            SchemaBuilder.AlterTable("ScheduleRecord",
                table => table
                    .AddColumn<string>("Type", c => c.WithLength(20)));

            return 2;
        }

        public int UpdateFrom2() {
            SchemaBuilder.AlterTable("ScheduleRecord",
                table => table
                    .AddColumn<string>("Signals"));

            return 3;
        }
    }
}