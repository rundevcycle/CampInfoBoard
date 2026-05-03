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
* `ViewModels/WeatherDisplayItem.cs`
* `WeatherEditorWindow.xaml`
* `WeatherEditorWindow.xaml.cs`



## Where We Left Off
Continuing the Camp Info Board app, part 5.

Please review `ChatGPT-instructions.md` and `copilot-instructions.md` first.

Current status:
- Multi-board save/open architecture works.
- Tides support manual editing and CSV import.
- Sunrise/sunset was converted from SunInfo to SunEntry list/table.
- Sun entries support manual editing, keyboard navigation, and CSV import.
- Display uses today’s SunEntry correctly.
- DisplaySettings now includes rotation timers, AlwaysOnTop, and MeasurementMode.
- WeatherBlock was modernized:
  - Date
  - WeatherPeriod enum: DayTime / NightTime
  - MeasurementDisplayMode enum: Metric / Imperial / Both
  - TemperatureC
  - FeelsLikeC
  - WindSpeedKph
  - WindGustKph
  - WindDirectionValue using WindDirection enum
  - UVIndex as int?
  - Description
  - PrecipitationDisplay
  - Icon
  - formatting helpers for temp / feels-like / wind
- Weather has a WeatherEditorWindow, not table editing.
- Weather display logic should show the next 3 entries:
  - Before 5 PM: Today, Tonight, Tomorrow
  - After 5 PM: Tonight, Tomorrow, Tomorrow Night
  - Overnight shift happens at 4 AM.
- Weather labels should stay generic:
  - Today
  - Tonight
  - Tomorrow
  - Tomorrow Night

Next direction:
I’m leaning toward keeping the dashboard weather widget simple and icon-based, but adding a new display mode, similar to Announcements/Photos, for detailed weather info.

Please start by reviewing the uploaded files and then help plan and implement the next step with small focused diffs.

*The next session should probably begin with `DisplayMode`, `DisplayViewModel`, and `DisplayWindow.xaml`.  Include weather classes and control window for good measure.*




## Other TODO Items
* DONE - Import tides from CSV
* DONE - Import sunrise/sunset from CSV
* Bilingual display 
    * English and French support to start
    * Primary language
    * Secondary language (optional)
* DONE - Table of weather forecasts - Day/Night resolution
* Weather icons
* Predefined weather descriptions with icons
* DONE - Allow freeform descriptions of weather
* Larger dashboard items
* Allow selection of which dashboard widgets to enable
