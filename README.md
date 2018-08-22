# GlitchWin
GlitchWin is a simple randomized glitch art lockscreen for Windows 7, heavily inspired by
[glitchlock](https://github.com/xero/glitchlock)

## Building
GlitchWin can be built with Visual Studio 2017 with the .NET 4.7.1 Targeting Pack or equivalent.
**You must run Visual Studio as Administrator.**

## Usage
Simply run GlitchWin.exe with no arguments to start the daemon. By default, the lockscreen is
updated every five minutes or when the session state changes.

### Limitations
Because of Windows limitations, the lockscreen can not be updated in time so that an up-to-date
screenshot is always generated like with glitchlock. If you know a way to do this, please file an
issue and I'll have a look.

Possible workarounds include:
* A screensaver (not quite the same thing as a lock screen)
* [UWP lock screen](https://docs.microsoft.com/en-us/uwp/api/windows.system.userprofile.lockscreen)
  APIs (I use Windows 7)
* Hooking into [SessionEnding](https://docs.microsoft.com/en-us/dotnet/api/microsoft.win32.systemevents.sessionending?view=netframework-4.7.2)
  (I might look into this at some point, as it stands it would require migrating from a console
  application to a Windows application, which would take some work)

## License
In the spirit of mandatory copyleft, I've licensed this program under the MS-RL.