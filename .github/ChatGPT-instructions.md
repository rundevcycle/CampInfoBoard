# ChatGPT Instructions

General AI coding instructions are found in `copilot-instructions.md`.  Include instructions there along with this.

## File Listing
The project is organized as follows:

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
Continuing CampInfoBoard (WPF / C#) public display app for campground info boards.

Please review `ChatGPT-instructions.md` and `copilot-instructions.md` first, then inspect the uploaded solution before suggesting changes.

Current architecture:
- Multi-board system with AppPaths + board folders
- JSON persistence via DataService with backups
- DisplayWindow + ControlWindow
- WeatherBlock modernized:
  Date, WeatherPeriod (DayTime/NightTime), MeasurementDisplayMode, TemperatureC, FeelsLikeC, WindSpeedKph, WindGustKph, WindDirectionValue, UVIndex, Description, PrecipitationDisplay, Icon
- WeatherEditorWindow with icon dropdown + preview
- Weather icons embedded as WPF Resources in Assets/WeatherIcons
- Compact dashboard weather widget + separate Detailed Weather display mode
- Weather rotation integrated into DisplayMode
- SunEntry list with sunrise/sunset display
- Sun display uses large sunrise/sunset icons
- TideEntries support chronological display:
  - Current tide if within 1 hour
  - Current + next 3 tides
  - Chronological order
  - Tide-high / tide-low icons
  - "Tomorrow"" separator only when needed
- FM frequencies integrated
- Schedule / Announcements / Photos functional

Current priority:
- Full presentation / polish review
- Focus on:
  * font hierarchy
  * spacing
  * widget alignment
  * icon consistency
  * section separation (borders / separators / cards)
  * large-screen readability
- Avoid major new functionality unless clearly beneficial
- Prefer small focused code snippets with exact placement
- No diff blocks with + signs
- Use braces on single-line if/for/foreach

First task in new session:
Review current `DisplayWindow.xaml` layout and help perform a full visual / presentation consistency pass.



## Presentation Polish Checklist
* [ ] Dashboard weather layout
* [ ] Detailed weather layout
* [ ] Schedule readability
* [ ] Announcement readability
* [ ] Sun/Tide visual refresh
* [ ] Font scale pass
* [ ] Spacing / padding pass
* [ ] Large-display distance test
* [ ] Section grouping / visual separation
* [ ] Borders vs separators
* [ ] Background panels / cards
* [ ] Widget spacing hierarchy
* [ ] Heading necessity review
* [ ] Contrast / opacity pass
* [ ] Widget alignment
* [ ] Font hierarchy
* [ ] Icon scale consistency
* [ ] Spacing rhythm
* [ ] Heading necessity
* [ ] Background image readability
* [ ] Border / section separation




## Other TODO Items
* Bilingual display 
    * English and French support to start
    * Primary language
    * Secondary language (optional)
* Version number
* Menu 
    * Move file operations from buttons to menu
    * User preferences (preferred units)
    * Help > About
* Import and export an info board to copy to another computer
* Background image selection and save with board folder
* Group FM frequencies in Dashboard admin tab
* Move Display Settings to bottom of Dashboard admin tab
* Move Tides & Sun tab to end, since they won't change very often if I do a bulk insert
* Announcements admin tab:
    * Display image preview instead of path
    * Simplify buttons: Add, Copy, Edit, Delete
* Announcement Edit window
    * Add note or tooltip explaining that bigger number = higher priority
    * Display image preview
    * Copy images into the board folder
    * Template codes, similar to OpenSong's square brackets
    * Some basic formatting in announcements (Markdown?)
    * Predefined announcement templates 
        * without an image
        * with an image
        * 1 column, 2 column
* Schedule Items
    * Include an optional image (use default if not provided?)
    * Larger and fewer in Details panel, with rotation
    * Colour coding for location, series, or topic (tags?)
* Weather admin tab:
    * Simplify buttons: Add, Edit, Copy, Delete
* Tides admin tab:
    * Simplify buttons: Add, Delete Old Entries
* Sun Times admin tab:
    * Simplify buttons: Add, Delete Old Entries
* Schedule admin tab:
    * Display description
    * Simplify labels: Add, Copy, Edit, Delete
    * Buttons in the same order as Weather & Announcements admin tab
* Windows icon
* Presentation
    * Don't need "Weather" label on weather details
    * Larger fonts throughout
        * Maybe allow for custom font sizes?
    * Dashboard widgets with borders, shading, or dividers
    * Larger text in weather widget
    * Larger text for date & time
    * Black outline for text?
    * Cleaner presentation of wind km/h vs mph in Weather Details
    * Featured Images go full screen, hiding the dashboard (help reduce screen burn-in)
