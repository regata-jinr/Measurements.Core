# Software core for control of the measurements process

## Introduction
This is the core of basic [regata](http://regata.jinr.ru/) software designed for control measurements process.

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

### Handlers

### Models

### SampleChanger

### Session

### SessionControllerSingleton
