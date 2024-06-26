## Introduction
WinManager is a utility which brings easier application and windows management to Microsoft Windows. WinManager enables efficient applications and windows switching and closing functionality that is optimized for the keyboard only and screen reader users. You will find the benefit of WinManager especially if you are running many applications and their open windows at the same time.

## Example usage
The benefit of WinManager can be best demonstrated on specific example of its usage. Let's assume you have open two Microsoft Word documents, four windows in the Microsoft Edge web browser, three File Explorer windows, and a Microsoft Outlook window with the inbox folder.

### Example 1: Windows switching or closing
Let's say you want to switch to or close certain window in Microsoft Edge where you searched something using Google:

1. Press Windows + F12, or Windows + Shift + A, to invoke WinManager. A list of four running applications mentioned above will be displayed.
2. Arrow down to the "Microsoft Edge" application, or simply just type "edge" to filter the list of applications.
3. Press the Right arrow key to show all windows for Microsoft Edge.
4. Arrow down to the desired Google search window, or narrow down the windows list just by typing "google".
5. Press Enter when on the desired window to switch to it, or press Delete to close it.

### Example 2: Applications quitting
Let's say you want to quit Microsoft Edge with all its windows so that it asks for restoring all the windows next time you start it:

1. Press Windows + F12, or Windows + Shift + A, to invoke WinManager. A list of four running applications mentioned above will be displayed.
2. Arrow down to the "Microsoft Edge" application, or simply just type "edge" to filter the list of applications.
3. Press Shift + Delete to force quit Microsoft Edge, which makes it ask you for restoring its windows next time you start it.

### Example 3: Windows switching or closing only for the application in foreground
Let's say you are in File Explorer, and want to switch or close its another "Downloads" window that you used a long time ago:

1. Press Windows + F11, or Windows + Shift + Q, to invoke WinManager. A list of open windows only for the application in foreground will be displayed, that is, for File Explorer.
2. Arrow down to the "Downloads" window, or simply just type "down" to filter the list of windows.
3. Press Enter when on the "Downloads" window to switch to it, or press Delete to close it.

## Features
After starting, WinManager notifies the user it has been started and then runs in background until its functionality is invoked using the corresponding keyboard shortcuts, which are global, that is, the shortcuts work no matter which application or window is currently in the foreground. The user can invoke the following lists:

* List of running applications and their open windows.
* List of open windows only for the application in the foreground.

### List of running applications and their open windows
After pressing Windows + F12, or Windows + Shift + A, the user can invoke a list of currently running applications, ordered by the most recently used application first. The list can be operated using the following keys:

* Down or Up arrow: Navigates to the next or previous item in the list.
* Right arrow: When an application is selected, navigates to the list of open windows only for that selected application.
* Left arrow: When a window is selected, navigates back to the list of running applications.
* Enter: When an application is selected, switches to that application's most recently used window. If a window is selected, switches to that window. Then hides WinSwitcher.
* Delete: When an application is selected, quits that application. If a window is selected, closes that window. TO force quit an application, press Shift + Delete. Note that force quitting an application may cause unsaved work to be lost, whereas application quitting or window closing may fail and require you to save work before closing.
* Escape: Hides WinManager.
* Alt + F4: Exits WinManager, so it no longer runs in background. WinManager notifies the user it is exiting.

### List of open windows only for the application in the foreground
After pressing Windows + F11, or Windows + Shift + Q, the user can invoke a list of open windows  only for the application in the foreground, ordered by the most recently used window first. The list can be operated using the following keys:

* Down or Up arrow: Navigates to the next or previous window in the list.
* Enter: When a window is selected, switches to that window. Then hides WinSwitcher.
* Delete: When a window is selected, closes that window. Note that window closing may fail and require you to save work before closing.
* Escape: Hides WinManager.
* Alt + F4: Exits WinManager, so it no longer runs in background. WinManager notifies the user it is exiting.

### WinManager settings
The WinManager settings can be accessed by the "Settings and more" button which can be navigated to using Tab or Shift + Tab on the main WinManager window.

After WinManager is launched, it registers itself so that it will be automatically launched every time you log in to Microsoft Windows. This auto launch behavior can be disabled in WinManager settings dialog.

### Applications or windows list filtering by typing
To make the finding of the desired application or window faster, the list of applications or windows supports filter by typing feature, which means that whenever the list is focused, typing characters immediately filters the list, displaying only the applications or windows whose titles contain the typed characters. The filtering is not case sensitive.

The filter can be reset and the original unfiltered list of applications or windows displayed back again by pressing Backspace.

### Changing the global keyboard shortcuts
The WinManager settings dialog allows you to configure the global keyboard shortcuts for invoking WinManager. More than one shortcut can be enabled at the same time, so that the chance of conflicting with another program is minimized.

For invoking the list of running applications, you can choose from the following shortcuts:

* Windows + F12 (checked by default).
* Windows + Shift + A (checked by default).

For invoking the list of open windows for the application in the foreground, you can choose from the following shortcuts:

* Windows + F11 (checked by default).
* Windows + Shift + Q (checked by default).

## Known limitations
* Occasionally, after an attempt to switch to a window, the focus is not properly placed to that window. What may help in this condition is pressing Alt + Tab and then Alt + Shift + Tab.

## Contact and feedback
If you have suggestions for WinManager improvement, problems with its functionality or other comments, you can drop me an email to [adam.samec@gmail.com](mailto:adam.samec@gmail.com)

## License
WinManager is available as a free and open-source software under the MIT license.

### The MIT License (MIT)

Copyright (c) 2024 Adam Samec

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
