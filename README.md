# Software core for control of measurements process

This is the core of basic [regata](http://regata.jinr.ru/) software.
<br>
<br>
We are using this software to manage spectra acquisition via GENIE2K Gamma Acquisition and Analysis program.

It has two types of programming interface: dlls and rexx.  Rexx is merely a wrapper for calling different exe files. So all of the so called batch support tools commands like putview or pvopen, it's just an exes, and we can call it directly from cmd.exe. 

We will use dll for control all processes of measurement, but also we will provide opportunity to show Genie2k main window in read-only mode for observing of measurement state.
