# Core of measurements software
This is the core of basic [regata](http://regata.jinr.ru/) software.
<br>
<br>
We are using as a basic software for a spectra acquisition GENIE2K Gamma Acquisition and Analysis. It has a two types of programming interface: dlls and rexx.  Rexx is merely a wrapper for calling exe files. So all of the so called batch support tools commands like putview or pvopen, it's just an exes, and we can call it directly from cmd.exe. We will use dll for control all processes of measurement, but also we will provide opportunity to show Genie2k main window in read-only mode, for observing for statuses.
<br>

