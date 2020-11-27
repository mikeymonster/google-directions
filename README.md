# Google Directions POC


### Directions API

 - https://developers.google.com/maps/documentation/directions/overview


### Places API

- https://developers.google.com/places/web-service/search
- https://developers.google.com/places/supported_types
- https://stackoverflow.com/questions/50313126/google-maps-api-search-for-nearest-train-station
- https://jdunkerley.co.uk/2016/07/21/geocoding-and-finding-nearest-station-with-google-web-services/

### Issues

- Had this error with the key used in test: **API keys with referer restrictions cannot be used with this API.** See https://developers.google.com/maps/faq#browser-keys-blocked-error for fix.


### TODO

- Add list of journey steps to the page
- look at bus vs train - do I need to call twice?
- Is it ok to take the time now, or do we need the more expensive "start time" option?

Directions API has a parameter to get just train and bus directions `transit_mode=train|bus

