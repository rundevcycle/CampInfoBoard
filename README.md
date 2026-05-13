# Camp Info Board
Display weather, tides, sun times, announcements, event schedules, and featured photos on a campground information board.


## Requirements

CampInfoBoard requires Windows and the .NET 10 Desktop Runtime.

If the app does not start, install the latest **.NET 10 Desktop Runtime** for Windows x64:

https://dotnet.microsoft.com/download/dotnet/10.0

Choose:
* .NET Desktop Runtime 10.x
    * Windows x64 Installer


# TODO
* [ ] Bilingual display



## Testing checklist (version 0.9.2)

Focus on:

1. Board/file workflow
    * [ ] New Board, Open Board, Save, Save As, Reload, Exit
    * [ ] Confirm auto-save works as expected
2. Display modes
    * [ ] Dashboard
    * [ ] Detailed weather
    * [ ] Schedule paging
    * [ ] Announcements
    * [ ] Full-screen photos
3. Dashboard toggles
    * [ ] Weather, sun, tides, FM radio
    * [ ] Confirm hidden widgets leave no empty cards
4. Images
    * [ ] Background image select/clear/change
    * [ ] Announcement image browse/clear/preview
    * [ ] Photo add/edit/delete
    * [ ] Confirm board folders clean up correctly after Save
5. Weather editor
    * [ ] Nullable fields can be cleared:
        * [ ] Feels Like
        * [ ] Wind Speed
        * [ ] Wind Gust
        * [ ] UV
    * [ ] Confirm display still looks right when optional fields are blank
6. Schedule
    * [ ] Past rows gray in admin table
    * [ ] Active event highlight on display
    * [ ] Events per page setting works
    * [ ] Future events show Today/Tomorrow/weekday/date correctly
7. Real screen test
    * [ ] Readability from distance
    * [ ] Rotation timing
    * [ ] Background color/image contrast
    * [ ] Photo caption size
    * [ ] Announcement paragraph readability