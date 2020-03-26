using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.UI.Navigation;
using Orchard.Localization;

namespace Orchard.Scheduler {
    public class AdminMenu : INavigationProvider {
        public AdminMenu() {
            T = NullLocalizer.Instance;
        }

        public string MenuName {
            get { return "admin"; }
        }

        public Localizer T { get; set; }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Scheduler"), "4.0", menu => menu.Action("Index", "Scheduler", new { area = "Orchard.Scheduler" }));
        }        
    }
}