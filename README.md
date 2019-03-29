# Measurements
Basic [regata](http://regata.jinr.ru/) software
<br>

### Developer diary
_upd. 29.03.2019_

Genie2K which we have like a basic software for a spectra acquisition has a two types of program interface. First of all it's dlls and rexx. Some time ago I found out that rexx is just a legacy from ibm. It is just a wrapper for calling exe files. So all of the so called batch support tools commands like putview or pvopen, it's just a exe, and we can call it directly from PS. Now it looks like preferable way for new solution. All cheks, statuses, processing etc will be now via dll, but main part like displaying of measurements via this exe files.

<br>

I splitted old Measurements project to two:
* MeasurementsCore
* MeasurementsUI



### Active ToDo list:

- [x] Design Detector class 
- [ ] Complete dll interface :
- [ ] The program should allow:
  - [ ] Add new detectors
  - [ ] Change name of detectors
  - [ ] Be able to cotrol detectors properties such as HV, etc (define the list of properties)
  - [ ] Remember the last state of measurement: what exactly it should remembers? Name of employee, type, time, ...
  - [ ] 