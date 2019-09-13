# Software core for control of the measurements process

## Introduction
This is the core of basic [regata](http://regata.jinr.ru/) software designed for control measurements process. We don't have plans to provide the general software. 

> **This core was implemented for exactly structure of experiment that we have. If you would like to use it, please, but the most probably you will need make plenty of changes.**

Our measurement process involved plenty internal processes:
* Loading information about the sample prepared for the measurement
* Setting detectors to the requred state (Type, Duration, Height, Note, ...)
* Setting sample to detectors via sample changer
* Acquring spectra to the files in parallel mode (now we have 5 HPGe detectors)
* Filling information about sample into the file of spectra
* Saving file spectra to the storage
* Saving information about measurements (file name, dates, ...) to the database
* Changing sample via sample changer

<br>

This core has implemented all of described point above.
The main goal of this core is make software independent from the interface.
Now we have desktop interfaces implemented via WinForms, but in plans we would like to migrate it to web interface based on ASP.NET.

### Developer's notes:

There are two types of programming interface to control acqusition process via Canberra devices: 
* Programming libraries (**S560**)
* Script based language Rexx(**S561**)  
> Rexx is merely a wrapper for calling different exe files. So all of the so called batch support tools commands like putview or pvopen, it's just an exes, and we can call it directly from cmd.exe. This is the way we used in the first version of the measurements automatization software

Now We will use dlls for control all processes of measurement, but also we will provide opportunity to show Genie2k main window in read-only mode for observing the state of measurement.

> **All classes have interfaces that they express. Programming your way based on these interfaces**

## Structure of the core

This core has next hierarchy:

Core
<br>
├──Detector<br>
├──Handlers<br>
├──Models<br>
├──SampleChanger<br>
├──Session<br>
└──SessionControllerSingleton<br>

Let's see to the each one more detailed.

### Detector

This is the main wrapper on CanberraDeviceAccessLib.DeviceAccessClass.
Detector class designed for controling real HPGe detector device and interaction with connected pc.
It allows to start, stop, clear, pause, continue measurements, save all measurement info to the file in the local storage.

### Handlers

This is additional tools for handling exceptions. It allows you to manage of occurred exceptions and wrap these  to any user interface form such as MessageBox or javascript Alert method.

### Models

Models are entities of real data. These models corresponds with EFcore models.

### SampleChanger
TBA

### Session

Session is the whole measurement process. In the frame of one session you able to controll certain detectors and samples. You can create few session and measure different samples on diffeMULTICHANNELrent detectors.

### SessionControllerSingleton

This is the control panel for managing of sessions. Using this one you can create, delete, attach|detach detectors to the created session. Load session from the DB.

## Getting Started

Typical macro that demonstrates how to use this core:

>**Before you start please pay attention that this software has determined data base structure that you can find in Models. Please, first of all create required tables and add data to it.**

~~~csharp
SessionControllerSingleton.InitializeDBConnectionString("YourConnectionString");
//here you can load session from db or you can create the new one
var iSession = SessionControllerSingleton.Load("name of saved session");
iSession.Type = "LLI-2";
iSession.CurrentIrradiationDate = DateTime.Parse("18.06.2012");
foreach(var m in iSession.MeasurementList)
    m.Note = "TEST!";

iSession.SetAcquireDurationAndMode(5);
iSession.StartMeasurements();
// here we are using pause, because measurements process has async nature inside. 
System.Threading.Thread.Sleep(iSession.Counts*iSession.IrradiationList.Count*1000 + iSession.IrradiationList.Count*1000);
~~~


## Additional tools

### Logging

We use NLog framework to keep information about internal processes. This allows you to find out the reason of some misleadings fast and easy. 

### Testing

For testing we use Xunit framework. We have unit testa that controls each method and functional tests for controls the process in general.

## Design of the core

Here we take a closer look to internal logic of the core.

First of all, the core has main controller class whick implemented singleton pattern. It has a name **SessionControllerSingleton** and provides to user opportunity to create or load session. Also it determines a connection string builder that other classes use for any manipulations with data base. This class possessed by the list of available detectors, which obtained from the MultiChannel Analyzer(MCA) databases of devices.

> Concerning of all terms corresponding with Gamma spectrometry or Canberra devices see any kinds of tutorial in your Genie2K folder, for example 'Genie2000 Tutorials Manual', etc.


The next class for logic order is **Session**. Session allows users to control any part of measurement process: start, stop, continue, pause, save spectra files, load info in database, serialize measurements, change samples on devices using sample changer(*has not implemented yet*). **Pay your attention: one session able to control any number of devices and user could create any number of session in order to make measurements in parallel**

For managing of devices Session uses Detector class. This is high level wrapper for the Canberra DeviceAccessClass. 


## Open problems

Here we will be keep the list of problems, which we would like to implement during the nearest time:

1. Now log files are creating on the disk storage, but in this way it impossible to create web-monitor for the tracking measurements state. Now the idea is to port logs from files into database and create simple web-monitor based on this data bases transactions


2. We think that the good idea is implement our own exception class that allow us to reach the better understanding in case of errors

3. S560 already has async structure, but the details are hidden. E.g. we cannot add timeout to connection, only via custom events (in case device ready message has not got during 10 sec  timeout exception will throw)
