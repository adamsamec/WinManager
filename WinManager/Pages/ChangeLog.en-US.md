# What's new in WinManager
## WinManager 1.0.4
* WinManager now detects all installed Windows OneCore voices and selects one based on current Windows language.
* Fix unable to switch to File Explorer from the apps list.
* Fix for wrong app being selected after window closing if app order changed.
* Fix order of File Explorer in apps list.

## WinManager 1.0.3
* Fix where for some users WinManager failed to launch. WinManager now uses voice installed in Windows instead of screen reader voice.

## WinManager 1.0.2
* Small text corrections and other changes for the help page.

## WinManager 1.0.1
* Added this "What's new in WinManager" page.

## WinManager 1.0.0
* WinSwitcher has been renamed to WinManager to better reflect its scope of features.
* Added support for switching to and closing of modern applications like Calculator and Settings.
* Added automatic check for update and download feature, which can be disabled in settings.
* Added possibility to either quit the selected running application using the Delete key, or force quit it using Shift + Delete. Normal quitting tries to close the application without risk of loosing unsaved work, failing when there are unsaved changes, while force quitting immediately terminates the application with the risk of loosing unsaved work.
* Added option in settings to disable or enable automatic WinManager launch on Windows startup.
* Fixed problem with keyboard shortcuts for invoking WinManager not always working.
