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


---


## Where We Left Off
Continuing CampInfoBoard WPF/C# public display app.

Please review `ChatGPT-instructions.md` and `copilot-instructions.md` first, then inspect the uploaded solution before suggesting changes.


Please review the attached CampInfoBoard files and read the session notes below before suggesting changes.  We recently completed a major display system refactor including semantic styles, live themes (Dark/Light/High Contrast), Markdown notes using Markdig + WebView2, current-event highlighting, and panel/style consolidation.  The display architecture is now resource-dictionary/theme based using DynamicResource semantic brushes.  Next priorities are field-test polish, validation/length limits, announcement templates/layout variants, and bilingual support.  Please inspect the current implementation before proposing changes and avoid guessing at existing code structure.


### CampInfoBoard — Session Notes
#### Completed This Session
##### Notes System
* Replaced MdXaml with:
    * Markdig
    * WebView2 preview
* Notes now support:
    * rendered Markdown
    * edit/preview toggle button
    * clickable external links
    * external browser launching via NavigationStarting
* HTML/CSS styling handled directly in generated HTML
* Compact heading spacing added
* WebView2 initialized with EnsureCoreWebView2Async()

##### Toast Notifications
* Added non-blocking save toast system
* Uses existing ShowToast() approach in ControlWindow
* Fixed layout shifting by using overlay positioning instead of layout flow

##### Display Style Consolidation
* Major cleanup of:
    * text styles
    * semantic typography hierarchy
    * semantic brushes
    * panel/card styles
* Reduced duplicate font sizes/styles
* Added:
    * `DisplayTinyTextStyle`
    * `DisplayMetaTextStyle`
    * `DisplayAccentMetaTextStyle`
    * etc.
* Converted most display text to shared styles

##### Themes

Implemented full live-switching theme system:

* Dark
* Light
* Dark High Contrast
* Light High Contrast

Architecture:

* ResourceDictionary-based themes
* DynamicResource brushes
* ApplyTheme() in DisplayWindow
* live refresh after ReloadData()

Theme files:
```
Themes/
    DarkTheme.xaml
    LightTheme.xaml
    DarkHighContrastTheme.xaml
    LightHighContrastTheme.xaml
```

Added semantic brushes:

* `DisplayTextBrush`
* `DisplayMutedTextBrush`
* `DisplayAccentTextBrush`
* `DisplayCardBrush`
* `DisplayStrongCardBrush`
* `DisplaySeparatorBrush`
* `DisplayReadabilityOverlayBrush`
* `DisplayCurrentEventBorderBrush`

##### Current Event Highlight
* Removed old converters
* Replaced with DataTrigger-based border highlighting
* Only active event shows border
* Removed blue background tint for readability
* Current event highlight now theme-aware

Removed:

* `ScheduleBackgroundConverter`
* `ScheduleBorderConverter`

##### Banner Improvements
* Added `BannerPanelStyle`
* Banner now sits inside themed panel
* Improved readability in Light themes


##### Background / Dirty Flag Fix
* Clearing background image now calls:

```c#
MarkBoardChanged();
```


##### UV Badge Cleanup
* Tightened UV label spacing using negative margins
* Kept large readable font sizes

##### Theme Dropdown Polish
* Added `EnumDisplayNameConverter`
* Displays:
    * Dark High Contrast
    * Light High Contrast
    * instead of enum names

#### Current Architecture State

Display system now has:

* semantic typography
* semantic brushes
* centralized panel styling
* scalable font sizing
* theme resource dictionaries
* live theme switching
* accessibility-ready structure

The display architecture is now in a strong maintainable state.

#### Next Likely Tasks
##### High Priority
* Field test at next weekend event
* Observe:
    * readability at distance
    * preferred themes
    * weather usefulness
    * banner visibility
    * current event highlighting
    * schedule pacing

##### Likely Next Feature

Announcement templates/layout variants:

* image-heavy
* text-heavy
* split layout
* minimal alert
* rotating styles

##### Validation / Guardrails

Start lightweight validation:

* MaxLength
* character counters
* overflow protection

First target:

* photo caption max length = 100


##### Possible Future Improvements
* Theme fine tuning after real-world testing
* Accessibility adjustments
* Additional note metadata
* Animation/transitions (low priority)
* Outdoor/daylight optimized theme
* Event-specific themes


#### Important Implementation Notes

##### Theme Refresh Order
Must be:

```c#
ReloadData()
RefreshTheme()
RestartRotation()
```

##### DynamicResource

Theme brushes MUST use:

```xml
{DynamicResource ...}
```

not `StaticResource`.


##### Photo Caption Exception

Photo captions intentionally remain white-based and are NOT theme-driven because they always render over a dark gradient overlay.



---

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
* [ ] Input validation (max length to start with)
