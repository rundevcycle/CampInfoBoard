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
* `Converters/NullableIntConverter.cs`
* `Converters/ScheduleBackgroundConverter.cs`
* `Converters/ScheduleBorderConverter.cs`
* `Converters/UVIndexToBrushConverter.cs`
* `DisplayWindow.xaml`
* `DisplayWindow.xaml.cs`
* `GlobalUsings.cs`
* `Models/Announcement.cs`
* `Models/AppData.cs`
* `Models/AppPreferences.cs`
* `Models/BannerPosition.cs`
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
* `Services/AppPreferencesService.cs`
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

Recent work completed:

* Reworked presentation controls into a clearer operator workflow.
* Replaced Show/Hide + Refresh with:

  * ▶ Start
  * ↻ Refresh
  * ■ Stop
* Added live presentation status indicator:

  * Offline
  * Live
  * Live – Pending Changes
* Refresh now only works from a saved state.
* Added snapshot-based unsaved-change detection using serialized AppData comparison.
* Added dirty tracking for collection changes (schedule, weather, announcements, photos, tides, sun, background image).
* Cleaned up false dirty prompts during startup by removing event-only dirty-state dependence.
* Presentation rotation logic now skips empty schedule mode and starts from the first available content mode.

Current presentation control layout:

* DockPanel-based layout.
* Monitor picker on the left.
* Status text + status dot near the presentation buttons on the right.
* Flexible spacing between monitor controls and presentation controls intentionally retained to allow wider status text.

Potential next areas:

* Presentation transition/fade animations.
* Large-display readability/distance pass.
* Presentation typography scaling/custom font sizing.
* Schedule grouping/highlighting in control tables.
* Bilingual display support.
* Announcement templating/formatting system.
* Additional presentation polish and operator UX refinement.
* Testing imported/exported boards on another machine


## Additional Instructions
* If you need to make changes in a file I haven't uploaded, don't guess; ask for it instead.
* Before moving on to something else, give me a short summary of the proposed changes before showing the code changes.  I may want to add something more, and limiting the amount of code displayed makes it easier to see where to make the changes.
* If there is potential ambiguitity with namespaces, add a comment or note as to which one I should use  e.g. System.Windows.Forms vs System.Windows.Controls.



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
* [X] Review all font & panel styles in the Display Window for consistency
    * [X] Create a list of all places where fonts are used, including size, colour, and opacity
    * [X] Create a list of all places where background panels are used, including size, colour, opacity, and borders
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
    * [ ] Alternating colour or highlight in control table to visually group events on the same day
    * [ ] Filter by speaker, title, or series
    * [X] Delete multiple rows
    * [X] Allow for rapid entry with default focus and overwrite
    * [X] Rapid entry of batches of events by keeping the window open and setting default times
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
* [X] Presentation
    * [X] Banner (top or bottom), always visible, but stylistically distinct from the schedule and announcements
    * [X] Don't need "Weather" label on weather details
    * [X] Larger fonts throughout
    * [X] Allow for custom font sizes
    * [X] Dashboard widgets with borders, shading, or dividers
    * [X] Larger text in weather widget
    * [X] Larger text for date & time
    * [X] Black outline for text?
        * Addressed by high contrast themes.
    * [X] Cleaner presentation of wind km/h vs mph in Weather Details
    * [X] Featured Images go full screen, hiding the dashboard (help reduce screen burn-in)
    * [X] Predefined themes
    * [X] Allow for custom themes?
        * No, we'll stick with the predefined themes.
    * [X] Hide weather widget if there's no weather
    * [X] Configure schedule for next X days instead of fixed at only 1 day.
* [ ] Create Demo board during installation
* [ ] Create installer
* [X] Control Window
    * [X] Add indicator to clearly show if the board is presenting live
    * [X] Separate Save from presentation buttons
    * [X] Add indicator if the currently presenting board is out of sync with the saved board.  I can still save changes while a board is presented, but let me know in case I want to bring the presentation in sync with my saved changes.
* [X] Add a notes tab for additional info, instructions when sharing, and links that are useful when populating a board (e.g. closest tide station, weather sources, event website with schedule, etc.).
    * [X] Commonmark markdown support
    * [X] Display rendered text by default, but allow for edit
    * [X] Default is "No notes."

