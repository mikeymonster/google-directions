# Google Directions POC


### Directions API

 - https://developers.google.com/maps/documentation/directions/overview


##### Directions API Billing 
- https://developers.google.com/maps/documentation/directions/usage-and-billing
- 0.005 USD per each (5.00 USD per 1000) - drops by 20% if over 100,000 queries a month
- Advanced - 0.01 USD per each (10.00 USD per 1000)

### Places API

- https://developers.google.com/places/web-service/search
- https://developers.google.com/places/supported_types
- https://stackoverflow.com/questions/50313126/google-maps-api-search-for-nearest-train-station
- https://jdunkerley.co.uk/2016/07/21/geocoding-and-finding-nearest-station-with-google-web-services/


##### Places API Billing 
- https://developers.google.com/maps/billing/gmp-billing#nearby-search
- Price starts at 0.032 USD per call, then Basic Data is free
- Fields - https://developers.google.com/places/web-service/place-data-fields#places-api-fields-support


### Issues

- Had this error with the key used in test: **API keys with referer restrictions cannot be used with this API.** See https://developers.google.com/maps/faq#browser-keys-blocked-error for fix.
- We are not authorised to use the Places API (I used a different key for testing)
- The API key can't be restricted to certain domains. I used our second key, but this might only work on localhost. Needs to be confirmed with the Development Manager.


### TODO

- Add list of journey steps to the page
- look at bus vs train - do I need to call twice?
- Is it ok to take the time now, or do we need the more expensive "start time" option?

Directions API has a parameter to get just train and bus directions `transit_mode=train|bus

- type of transport is just a suggestion. If there is only a bus, there's no point in asking for a train - it will just give you the bus again
- the journey will vary during the day. We probably need to specify an ideal departure or arrival time (at double the cost)
- lots of other types of transport:
  >* RAIL	Rail.
  >* METRO_RAIL	Light rail transit.
  >* SUBWAY	Underground light rail.
  >* TRAM	Above ground light rail.
  >* MONORAIL	Monorail.
  >* HEAVY_RAIL	Heavy rail.
  >* COMMUTER_TRAIN	Commuter rail.
  >* HIGH_SPEED_TRAIN	High speed train.
  >* LONG_DISTANCE_TRAIN	Long distance train.
  >* BUS	Bus.
  >* INTERCITY_BUS	Intercity bus.
  >* TROLLEYBUS	Trolleybus.
  >* SHARE_TAXI	Share taxi is a kind of bus with the ability to drop off and pick up passengers anywhere on its route.
  >* FERRY	Ferry.
  >* CABLE_CAR	A vehicle that operates on a cable, usually on the ground. Aerial cable cars may be of the type GONDOLA_LIFT.
  >* GONDOLA_LIFT	An aerial cable car.
  >* FUNICULAR	A vehicle that is pulled up a steep incline by a cable. A Funicular typically consists of two cars, with each car acting as a counterweight for the other.
  >* OTHER	All other vehicles will return this type.
 

TODO: Convert all distances into miles
Use alternative routes to find closes train and closest bus
Search for arrival time


### Bing/Azure API:

   https://codematters.online/how-bing-maps-api-features-compare-to-google-maps/
   Bing Routes API - https://docs.microsoft.com/en-us/rest/api/maps/route
   https://azure.microsoft.com/en-gb/pricing/details/azure-maps/
   https://www.infoworld.com/article/3346086/how-azure-maps-differs-from-bing-maps-for-developers.html
 
