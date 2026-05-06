# ChatGPT Instructions

General AI coding instructions are found in `copilot-instructions.md`.  Include instructions there along with this.

## File Listing
The project is organized as follows:

* `AboutWindow.xaml`
* `AboutWindow.xaml.cs`
* `AnnouncementEditorWindow.xaml`
* `AnnouncementEditorWindow.xaml.cs`
* `App.xaml`
* `App.xaml.cs`
* `AssemblyInfo.cs`
* `BoardNamePromptWindow.xaml`
* `BoardNamePromptWindow.xaml.cs`
* `ControlWindow.xaml`
* `ControlWindow.xaml.cs`
* `Converters/BoolToVisibilityConverter.cs`
* `Converters/ImagePathToBitmapConverter.cs`
* `Converters/NullToVisibilityConverter.cs`
* `Converters/ScheduleBackgroundConverter.cs`
* `Converters/ScheduleBorderConverter.cs`
* `Converters/UVIndexToBrushConverter.cs`
* `DisplayWindow.xaml`
* `DisplayWindow.xaml.cs`
* `GlobalUsings.cs`
* `Models/Announcement.cs`
* `Models/AppData.cs`
* `Models/DisplayMode.cs`
* `Models/DisplaySettings.cs`
* `Models/MeasurementDisplayMode.cs`
* `Models/PhotoItem.cs`
* `Models/RadioInfo.cs`
* `Models/ScheduleItem.cs`
* `Models/SunEntry.cs`
* `Models/TideEntry.cs`
* `Models/TideInfo.cs`
* `Models/TideType.cs`
* `Models/WeatherBlock.cs`
* `Models/WeatherPeriod.cs`
* `Models/WindDirection.cs`
* `OpenBoardWindow.xaml`
* `OpenBoardWindow.xaml.cs`
* `PhotoEditorWindow.xaml`
* `PhotoEditorWindow.xaml.cs`
* `ScheduleItemEditorWindow.xaml`
* `ScheduleItemEditorWindow.xaml.cs`
* `Services/AppPaths.cs`
* `Services/DataService.cs`
* `Services/PhotoFileService.cs`
* `Services/SunImportService.cs`
* `Services/TideImportService.cs`
* `ViewModels/DisplayViewModel.cs`
* `ViewModels/PhotoManagerViewModel.cs`
* `ViewModels/TideDisplayItem.cs`
* `ViewModels/WeatherDisplayItem.cs`
* `WeatherEditorWindow.xaml`
* `WeatherEditorWindow.xaml.cs`




## Where We Left Off
Continuing CampInfoBoard WPF/C# public display app.

Please review `ChatGPT-instructions.md` and `copilot-instructions.md` first, then inspect the uploaded solution before suggesting changes.

Current status:
- Multi-board system with AppPaths board folders.
- JSON persistence with backups.
- Menu bar added: New Board, Open Board, Reload, Save, Save As, Exit, Help > About.
- Version number/About window added.
- Auto-save on board switch/exit.
- Display settings moved to Settings tab.
- Dashboard toggles include weather, sun, tides, FM; date/time always visible.
- Display supports background image copied into Background folder, solid background color picker, and cleanup of unused background-* files.
- Featured photos are full-screen on black; background image/overlay hidden during photo mode.
- Weather dashboard and detailed weather polished, with UV color badge.
- Schedule mode polished: 3-line time block, active event highlight, paged schedule rotation, configurable ScheduleEventsPerPage.
- Announcements polished: card layout, image thumbnails in table, image preview in editor, Browse/Clear image, images copied into Announcements folder, cleanup of unused announcement-* files.
- Admin DataGrid rows for past weather/tides/sun/schedule show gray/dimmed.
- Nullable weather numeric fields use NullableIntConverter so blank text sets int? values back to null.

Recent thing fixed:
- Clearing nullable numeric weather values now works using NullableIntConverter.

Next likely priorities:
1. Thorough testing and bug fixes from field testing.
2. Import/export full board package.
3. More admin cleanup/reordering if needed.
4. Optional empty-state messages.
5. Bilingual display support later.



## Presentation Polish Checklist
* [X] Dashboard weather layout
* [X] Detailed weather layout
* [X] Schedule readability
* [X] Announcement readability
* [X] Sun/Tide visual refresh
* [X] Font scale pass
* [X] Spacing / padding pass
* [ ] Large-display distance test
* [X] Section grouping / visual separation
* [X] Borders vs separators
* [X] Background panels / cards
* [X] Widget spacing hierarchy
* [X] Heading necessity review
* [X] Contrast / opacity pass
* [X] Widget alignment
* [X] Font hierarchy
* [X] Icon scale consistency
* [X] Spacing rhythm
* [X] Heading necessity
* [X] Background image readability
* [X] Border / section separation




## Other TODO Items
* [ ] Bilingual display 
    * [ ] English and French support to start
    * [ ] Primary language
    * [ ] Secondary language (optional)
* [X] Version number
* [ ] Menu 
    * [X] Move file operations from buttons to menu
    * [ ] User preferences (preferred units)
    * [X] Help > About
* [X] Import and export an info board to copy to another computer
* [X] Background image selection and save with board folder
* [X] Group FM frequencies in Dashboard admin tab
* [X] Move Display Settings to bottom of Dashboard admin tab
* [X] Move Tides & Sun tab to end, since they won't change very often if I do a bulk insert
* [X] Announcements admin tab:
    * [X] Display image preview instead of path
    * [X] Simplify buttons: Add, Copy, Edit, Delete
    * [X] Delete multiple rows
* [ ] Announcement Edit window
    * [X] Add note or tooltip explaining that bigger number = higher priority
    * [X] Display image preview
    * [X] Copy images into the board folder
    * [ ] Template codes, similar to OpenSong's square brackets
    * [ ] Some basic formatting in announcements (Markdown?)
    * [ ] Predefined announcement templates 
        * [ ] without an image
        * [ ] with an image
        * [ ] 1 column, 2 column
* [ ] Schedule Items
    * [ ] Include an optional image (use default if not provided?)
    * [X] Larger and fewer in Details panel, with rotation
    * [ ] Allow tagging to group related events
    * [ ] Colour coding for location, series, or tags
    * [ ] Alternating colour or highlight to visually group events on the same day
    * [ ] Filter by speaker, title, or series
    * [X] Delete multiple rows
* [X] Photos admin tab:
    * [X] Delete multiple rows
* [X] Weather admin tab:
    * [X] Simplify buttons: Add, Edit, Copy, Delete
    * [X] Delete multiple rows
* [X] Tides admin tab:
    * [X] Simplify buttons: Add, Delete Old Entries
* [X] Sun Times admin tab:
    * [X] Simplify buttons: Add, Delete Old Entries
* [ ] Schedule admin tab:
    * [ ] Display description
    * [X] Simplify labels: Add, Copy, Edit, Delete
    * [X] Buttons in the same order as Weather & Announcements admin tab
* [X] Windows icon
* [ ] Presentation
    * [X] Banner (top or bottom), always visible, but stylistically distinct from the schedule and announcements
    * [X] Don't need "Weather" label on weather details
    * [X] Larger fonts throughout
    * [ ] Allow for custom font sizes
    * [X] Dashboard widgets with borders, shading, or dividers
    * [X] Larger text in weather widget
    * [X] Larger text for date & time
    * [ ] Black outline for text?
    * [X] Cleaner presentation of wind km/h vs mph in Weather Details
    * [X] Featured Images go full screen, hiding the dashboard (help reduce screen burn-in)
    * [ ] Predefined themes
    * [ ] Allow for custom themes?
    * [X] Hide weather widget if there's no weather
* [ ] Create Demo board during installation
* [ ] Create installer
* [ ] Control Window
    * [ ] Add indicator to clearly show if the board is presenting live
    * [ ] Separate Save from presentation buttons
    * [ ] Add indicator if the currently presenting board is out of sync with the saved board.  I can still save changes while a board is presented, but let me know in case I want to bring the presentation in sync with my saved changes.


