# Camp Info Board
Display weather, tides, sun times, announcements, event schedules, and featured photos on a campground information board.


# TODO

* :ok: Import tides from CSV
* :ok: Import sunrise/sunset from CSV
* :o: Bilingual display
* :ok: Table of weather forecasts - Day/Night resolution
* :ok: Weather icons
* :ok: Predefined weather descriptions with icons
* :ok: Allow freeform descriptions of weather
* :ok: Larger dashboard items
* :ok: Allow selection of dashboard widgets
* :ok: Version number
* :ok: Menu for user settings, file operations, help > about



# Miscellaneous Notes & Links

## Tides
Tides, sunrise, sunset, moonrise, moonset for up to 30 days.  
https://www.tide-forecast.com/tide/Pugwash-Nova-Scotia/tide-times

Even better, but need to manipulate the CSV file a little bit.
Run for AST and adjust as needed for ADT.
https://www.tides.gc.ca/en/stations/01775/predictions/annual


## Sun Times
Sunrise/Sunset, but need to adjust for ADT.
https://nrc.canada.ca/en/research-development/products-services/software-applications/sun-calculator/

Pugwash Coordinates
* 45.87305618035307, -63.55239507407187
* 45°52.3834' N, 63°33.1437' W
* 45°52'23.0" N, 63°33'08.6" W

## UV Forecast
4-day UV Forecast
https://www.theweathernetwork.com/en/city/ca/nova-scotia/pugwash/uv




## Source Code Listing
```bash
find . \( -path "*/bin" -o -path "*/obj" \) -prune -o \( -iname "*.cs" -o -iname "*.xaml" \) -print | sort | sed 's/\.\//\* /'
```



## Testing checklist (version 0.9.0)

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