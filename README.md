Cart Plugins - Cart API Engine for C#
=====================================

Cart API encoding and decoding engine for C#. Used by cart plugins implementing the [Cart API protocol]. If you are developing a new cart plugin in a C#-based environment, use the C# classes provided here to encode and decode the various Cart API protocol entities.

I/O related functions (like socket write) are implemented as `async` functions. If your code is not using async functions, you can easily change all the async functions into their regular sync counterparts.

Classes
-------

* `Appixia.Engine.Engine` - The general engine class, use this class when handling a Cart API request. Contains utility functions that handle a new request, parse the request metadata and initialize the correct encoder/decoder classes according to the metadata. For handling a new Cart API request, all you have to do is use the function `Engine.HandleRequestAsync()` which parses all the relevant arguments and initializes the correct encoder.
* `Appixia.Engine.Helpers` - Class containing various static helper utility functions. For example, use `Helpers.CreateSuccessResponse()` to create a successful response to the Cart API request, or use `Helpers.DieOnErrorAsync()` to create a failure response and exit.

Mediums
-------

Cart API supports various mediums for encoding/decoding protocol calls. For example, XML over HTTP POST and JSON over HTTP GET are two different mediums. For more information about mediums see [Cart API mediums reference].

* `XML` - Implementation of encoder/decoder for the XML over HTTP POST medium.
* `JSON` - Implementation of encoder/decoder for the the JSONP over HTTP GET medium.

Internal Classes
----------------

* `Appixia.Engine.Mediums.Encoder` - Base class for an encoder. All encoders (for the various supported mediums) inherit from it. Should not be used directly.

  [Cart API protocol]: http://kb.appixia.com/cartapi:ver1:introduction
  [Cart API mediums reference]: http://kb.appixia.com/cartapi:ver1:mediums
