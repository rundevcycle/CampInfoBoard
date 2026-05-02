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
* `Models/PhotoItem.cs`
* `Models/RadioInfo.cs`
* `Models/ScheduleItem.cs`
* `Models/SunInfo.cs`
* `Models/TideEntry.cs`
* `Models/TideInfo.cs`
* `Models/TideType.cs`
* `Models/WeatherBlock.cs`
* `OpenBoardWindow.xaml`
* `OpenBoardWindow.xaml.cs`
* `PhotoEditorWindow.xaml`
* `PhotoEditorWindow.xaml.cs`
* `ScheduleItemEditorWindow.xaml`
* `ScheduleItemEditorWindow.xaml.cs`
* `Services/AppPaths.cs`
* `Services/DataService.cs`
* `Services/PhotoFileService.cs`
* `ViewModels/DisplayViewModel.cs`
* `ViewModels/PhotoManagerViewModel.cs`


## Where We Left Off
CampInfoBoard WPF project continuation — Part 4

### Current project status:
* Multi-board architecture complete
    * Default / Save Board As / Open Board
    * Board folders:
        * Data
        * Photos
        * Backups
* JSON persistence + automatic backups
* Photo system:
    * Add / Copy / Edit / Delete
    * DisplayOrder
    * Move Up / Down
    * Save-time orphan photo cleanup
    * Non-locking image converter for thumbnails + display
* DisplayWindow:
    * Fullscreen borderless
    * Selectable monitor via DisplayMonitorIndex
    * Caption / Credit overlay
    * No-photo fallback
* DisplaySettings currently includes:
    * ShowTides
    * ShowSun
    * ShowUV
    * DisplayMonitorIndex

### Next priorities:
1. Settings expansion:
    * PhotoRotationSeconds
    * AnnouncementRotationSeconds
    * DisplayAlwaysOnTop
2. Settings UI panel
3. Later:
    * User preferences (home folder, Celsius/Fahrenheit)
    * Optional topmost toggle
    * Transition polish

### Important:
* Match existing architecture
* Review uploaded solution files first
* Avoid unnecessary rewrites
* Step-by-step code changes only


## Other TODO Items
* Import tides from CSV
* Import sunrise/sunset from CSV
* Bilingual display 
    * English and French support to start
    * Primary language
    * Secondary language (optional)
* Table of weather forecasts - Day/Night resolution
* Weather icons
* Predefined weather descriptions with icons
* Allow freeform descriptions of weather
* Larger dashboard items
* Allow selection of which dashboard widgets to enable
