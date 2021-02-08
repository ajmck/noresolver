# Night Ops Resolver Tool

This project was an internal tool to act as a single point of contact between our ticketing system (BMC Remedy / ITSM) and our internal device monitoring site. 

This repository has been sanitised of identifying information and code and will not compile as is, full unsanitised repo remains with the company.

![Screenshot of Night Ops Resolver Tool](docs/noresolver.png)


## Context

To manage high priority incidents, a search query is run in ITSM to see P1/P2 alerts, which are either active incidents under investigation, or alerts raised by our device monitoring tool which need further investigation (ie: a ping fail could be power down at a site, or a network fault).

The existing process requires a large amount of manual data entry:

![Animation of existing process](docs/itsm-animation.gif)

This project acts as a single point of contact for these two sites, prefetches the device history as needed, and reduces the amount of manual work to enter data and resolve tickets:

![Animation of Night Ops Resolver Tool](docs/noresolver-anim.gif)

Or, comparing the two processes as diagrams:


### Existing Workflow

![Existing Workflow Diagram](docs/workflow.png)


### Revised Workflow

![Revised Workflow Diagram](docs/new_workflow.png)


## Extra Features

For high-priority management, we also have integration in with the on call engineer roster, browser tabs showing the intranet's priority incident dashboard, and a set of contacts (engineers / service desks / service delivery managers) which are crossreferenced with each incident.

The features which I was unable to implement due to requiring work on the ITSM API from another stakeholder were subbing incidents, viewing service level agreements, and placing incidents in to a 'pending' state. The workflow diagram above represents the ideal state, and the existing ITSM site is still available for any tasks which can't be completed in Night Ops Resolver Tool.

## Project Architecture

`NoResolver.Core` is a shared library. It connects to ITSM via the SOAP API provided by BMC Remedy, scrapes the HTML from SMF's device history, and performs all that can be done without a UI.

`NoResolver.WPF` is a Windows Presentation Foundation (WPF) application, using the ModernWPF library for style purposes, and the Prism library to follow the MVVM architecture pattern. An Universal Windows Platform application was the initial plan, but became infeasible for distribution reasons.

`NoResolver.OnCall` is a shared library dedicated to the NOC oncall roster - was a bit too disparate to keep in Core

`NoResolver.CLI` is handy for testing new endpoints

An extended description of the project and its files / architecture is available in the wiki.


## Licence

This repository has been sanitised of company information and released under a MIT licence.

Please note that this project will not run without a reasonable amount of modification, as it's connected to endpoints which are only accessible via intranet, and the links to these endpoints have been removed.


## What's been removed?

Couple of things, most have been marked with the comment `/* REMOVED (- comment) */`

Use `rgrep REMOVED` in a CLI to see a summary

* Hostnames of intranet sites
* Search queries run in BMC Remedy
* WDSL output from BMC Remedy
    * Autogenerated files have been kept, because some modification was required on these files (some fields had to be made nullable)
* Excel doc outlining ITSM Schema and database fields - available at http://www.softwaretoolhouse.com/freebies/index.html
* `assignees.json` and `contacts.csv` - staff names and contacts loaded at runtime
    * N.B. these were not saved within the primary repo in the first place


# Debrief

This project was created within a corporate environment, which meant some design choices had to be made.

To distribute the project to other users, the software had to compile to an EXE. This ruled out creating a UWP application, as these packages are distributed as an .msix file and must be digitally signed. XAML Islands looked promising, but was not available on our build of Windows when I started (and also requires Developer Mode to be enabled). WinUI would be the ideal approach now, but it was not available when I started the project, and would have required a beta install of Visual Studio.

Therefore, I created a Windows Presentation Foundation project, using the ModernWPF UI library for style purposes.

I started the project by creating a small CLI program to act against the endpoints required. I had some difficulty getting the search query to work, until I found that the database fields should be referenced by ID instead of name (refer to the excel doc mentioned above). 

I also encountered some issues serialising the XML output to C# objects; this turned out to be an issue with BMC Remedy, which I solved by intercepting the XML and removing the empty elements which were causing the runtime exception.

Originally, our device monitoring site used a simple username and password for access. This was then changed to use a tokencode instead of a password. At one point the project was changed to access the device history over a SSH host instead of the web browser, but as this required a different set of credentials not all users would have, the project began accepting tokencodes (main problem here was persisting credentials, which was a matter of storing cookies)

Another concern was the amount of stress this tool was placing on our device monitoring site, as originally it would request the device history from every entry, and refreshing the list would start checking devices in another thread. Certain devices with a large amount of events would cause the application to hang, and there were times the site appeared to be thrashed. This was ultimately resolved by only checking the devices when they were double clicked, and adding these requests to a queue so only one device could be checked at a time, reducing the stress on the monitoring site.

Some of the endpoints from BMC Remedy would work fine when testing in SoapUI, but not in the C# project. The endpoint would accept null values, but this wasn't reflected in the autogenerated code. Therefore, some fields in the autogenerated code had to be modified to allow null values.

There were some features which were constrained by what endpoints I had available in BMC Remedy - this would require work from the BMC admin at the company, but I handed over the project to the company before this point. For one of these features (viewing service level agreements), I scaffolded the code required in the branch `sla-ui` and created an implementation path in the wiki for project continuity.

There was a stakeholder concern about how passwords were stored at rest, so credentials were stored in Windows Credential Manager using a nuget library.

One thing I would have liked is more error feedback to users and the implementation of test cases, however this was constrained by the time I had available and the fact I was using a third pary UI library. 

I was mostly able to follow a Model-View-ViewModel pattern for the UI, but some elements of it became complicated by using a third party library (vs a stock UWP application with plenty of open source examples to compare against). Therefore there are some places where this pattern becomes a bit muddled (i.e. some static references to other viewmodels)

I would also probably tidy up the WPF/Views folder, as many of those files are individual controls rather than whole views.

One handy feature ended up being the modal popup in the corner of the screen when a new alert comes in, as it meant a reminder that there was something to be checked (vs the old way of needing a browser tab front and centre the whole time).

The last set of features I implemented was pulling in our engineer roster and other contacts, as this typically required going to another site or knowledge base to search for their number. This app allows you to see relevant contacts straight from the incident and copy their number to the clipboard by double clicking (to be pasted in to a softphone dialer)

Ultimately, the feedback from my team was positive, because this tool vastly lifted the manual effort required for one of the core parts of our role.
