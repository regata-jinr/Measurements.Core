# Design
In this document I will keep information about design status and problems.
<br>
Implementation problems will be in readme.md
<br>

First of all lets describe the content of this document:
<br>
### User roles
### Core
#### Classes and Interfaces
##### Fields
##### Properties
### Forms
#### MainForm
##### Controls
##### View
##### Format
### Logs
### Exceptions

<br>

Before we start, I would like to notice, that we will try to use user story and user cases for projecting. Based on this things we will create CRC cards.
<br>

 

#### Class Detector
Detector is one of the main class, because detector is the main part of our experiment. It should be the most effective in all components: perfomance, safety, and so on. The good idea is add the implementation of auto calibration.

#### Fields:
Satus - {ready, off, busy, error}


The basic part is:

1. Manage of all detectors like one;
2. Show mvcg window
3. Save information to file
4. Save information about file in DB
5. Save file to ftp server