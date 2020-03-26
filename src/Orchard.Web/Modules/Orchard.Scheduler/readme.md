# Scheduler
An [Orchard CMS](http;//orchardproject.net) that enables recurring scheduled tasks within the Orchard framework. The module integrates with the Workflow to fire signals, which can start or resume workflows.

## Integration with Workflow module
Once enabled the module adds a Scheduler menu item to the admin menu, from this screen a site owner can create new Schedules based on the crontab format. The Scheduler form has 2 modes; Basic and Advanced. The Basic form limits input via drop down lists and checkboxes, where the Advanced mode allows more liberal input through the use of text inputs. More information on creating more complex expressions can be found at the [ncrontab home page](http://www.raboof.com/projects/ncrontab/). 

These Schedules can then be used as an event to trigger a Workflow. Add a signal event in your workflow and then use the name of the signal in your scheduled task. Everytime the schedualed task is fired it will trigger the signal to start/resume your workflow.

Many thanks to [raboof](https://twitter.com/raboof) for creating the ncrontab library.
