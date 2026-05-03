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
* Some basic formatting in announcements
* Predefined announcement templates with or without a picture
* Background image selection and save with board folder



