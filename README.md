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
* :o: Larger dashboard items
* :ok: Allow selection of dashboard widgets
* :o: Version number
* :o: Menu for user settings, file operations, help > about



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

