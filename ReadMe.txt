'Regarding You Performance' is a utility to monitor the performance 
of a collection of remote hosts. It consists of two parts: the Agent, which 
runs as a Windows service and responds to requests for information in JSONP
format via a built in HTTP service, and the client, which is a web page which 
polls the various agents at regular intervals and graphs the results.

The values with are included in the monitoring data are the processor usage, 
the amount of free physical memory and the number of ASP.NET requests per 
second.

AGENT INSTALLATION

To install the agent you need to start a command prompt as Administrator and 
then run the agent with the -i command line argument, like so:

Performance.Agent -i

This will install the service. It can then be started using the following 
command:

net start Performance.Agent.Service

To uninstall the agent use:

Performance.Agent -u

The port number the agent will serve requests on is specified in the config 
file (Performance.Agent.exe.config), along with the time between performance
snapshots, in milliseconds.

CLIENT SETUP

The client is a single HTML page which receives its updates via AJAX from 
the various agents. For register an agent with the client you need to change
the following lines of the HTML:

var feedData = [
    { url : 'http://localhost:7812/', name : 'test' }
];

You can add another agent to the list like so: 

var feedData = [
    { url : 'http://localhost:7812/', name : 'Test' },
    { url : 'http://www.someotherhost:7812/', name : 'Some Other Host' }
];

SUPPORTED

Tested on:

Windows 7 64-bit Home Profession
Windows Server 2003 Standard SP1

TODO

- Add 'current requests'
- Add hosted webpage client option
