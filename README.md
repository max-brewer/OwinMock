# OwinMock
A ridiculously simple mock server using OWN 

Sets up a server with canned responses for integration testing.

## ToDo

* Needs to work with all methods
* Work with multiple ports from one owin mock? Then it cleans them all up together.
* Which methods have request bodies? - Maybe have various request builders for different request types.
* Need to be able to interrogate the request for matching.
	* On query parameters
	* On headers
	* On request body
* Need to record the requests. - Then you can have a look later/do some assertions.
* Maybe get rid of dependencies on Microsoft.Owin and Owin.Hosting (Owin.Hosting can be a dependency, but not of the core module).
* Https?
* Various kinds of auth?
* Various helper methods for different request types
* Fluent API