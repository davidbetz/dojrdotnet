# Dojr.NET Dojo RPC Library

Dojr.NET is a simple framework created to allow easy creation of dojo.rpc services by defining a class which inherits from a specific base class, adding one or more attributes, and registering it as an HttpHandler.

## Description

Dojr.NET is built on .NET 3.5 technology and attempts to give WCF-like usability. Dojr.NET actually internally uses DataContractJsonSerializer which WCF 3.5 internally uses for its own JSON serialization. Dojr.NET, like WCF, uses attributes to declare specific service operations. However, unlike WCF, Dojr.NET inherits from a class, like COM+, where as it is idiomatic in WCF to implement a service contract interface. Dojr.NET creates each service as an HttpHandler which is then registered to ASP.NET at a particular endpoint.

*(following was written circa ~2007/2008 by David Betz; initially at [netfxharmonics.com](https://netfxharmonics.com/n/2008/01/dojrnet-dojo-rpc-library-net-20))*

## Background / Context : Using dojo.rpc

Dojo provides a nice service abstraction layer in the form of dojo.rpc.  This is an absolutely astounding feature, yet it's so simple.  Instead of making all kinds of functions and setting up and XHR object, Dojo allows you to call server methods using a very simplified syntax.  The model should be familiar to anyone who has worked with SOAP services.  In these types of services, you are given a scheme and, depending on what client you are using, you can create a client-side proxy for all interaction with the service.  This is how the dojo.rpc feature works.  When you want to access a service, give Dojo the appropriate service metadata it needs to create a proxy and just call your service functions on the proxy.


In Dojo, this schema is called a **Simple Method Description (SMD)** and looks something like this.
	
	var d = {
	  'methods':
	    [
	      {
	        'name':'getServerTime',
	        'parameters':[
	          {'name':'format'}
	        ]
	      },
	      {
	        'name':'getServerTimeStamp',
	        'parameters' :[
	        ]
	      }
	    ],
	    'serviceType':'JSON-RPC',
	    'serviceURL':'/json/time/'
	}

With this SMD data, you create a proxy by getting and instance of the dojo.rpc.JsonService object setting the SMD in the constructor, like this:

	var timeProxy = new dojo.rpc.JsonService(d);

From here you can call methods on the proxy and set a callback:

	timeProxy.getServerTimeStamp( ).addCallback(function(r) { alert(r); });

Upon execution, this line will call the `getServerTimeStamp` method described in the SMD and route the output through the anonymous function set in the `addCallback` function.  If you would like, however, you can defer the callback by calling the service now and explicitly releasing the callback later.  In the following example, the first line calls the server immediately and the second releases the callback.

	var deferred = timeProxy.getServerTimeStamp( );

	deferred.addCallback(function(r) { alert(r); });

This is great, but what about the server?  As it turns out, Dojo, sends JSON to the service.  You can see this for yourself by taking at keep at the `Request.InputStream` stream in ASP.NET:

	StreamReader reader = new StreamReader(Request.InputStream);
	String data = reader.ReadToEnd( );

Below is the data that was in the stream.  As you can see, this is extremely simple.

	{\"params\": [], \"method\": \"getServerTimeStamp\", \"id\": 1}

## Technical Prerequisite: providing Server Functionality
Since we are working in .NET, we have at our disposal many mechanisms that can help us deal with various formats, some of which that can really help simplify life.  As I explained in my XmlHttp Service Interop Series, providing communication between two different platforms isn't at all difficult, provided that you understand the wire format in between them.  In part 3 of that same series, I explained how you could use XML serialization to quickly and powerfully interop with any XML service, including semi-standard a SOAP service.  Furthermore, you aren't limited to XML.  Provided the right serializer, you can do the same with any wire format.  For our purposes here, we need a JSON serializer.  One of my favorites is the Json.NET framework.  However, to keep things simple and to help us focus more on the task at hand, I'm going to use the .NET 3.5 `DataContractJsonSerializer` object.  If you are working in a .NET 2.0 environment with a tyrannical boss who despises productivity, you should check out Json.NET (or get a new job).

To begin our interop, the first thing we need is a type that will represent this JSON message in the .NET world.  Based on what we saw in the ASP.NET Input Stream, this should be easy enough to build:
	
	[DataContract]
	public class DojoMessage
	{
	    [DataMember(Name = "params")]
	    public String[] Params;
	
	    [DataMember(Name = "method")]
	    public String Method;
	
	    [DataMember(Name = "id")]
	    public Int32 Id = 0;
	}

Having that class in place, we can now deserialize ASP.NET's InputStream into an instance of this class using out DataContractJsonSerializer:

	DataContractJsonSerializer s = new DataContractJsonSerializer(typeof(DojoMessage));
	DojoMessage o = (DojoMessage)s.ReadObject(stream);

That's it.  Now you have a strongly typed object where you can access the method and parameter information as you need.  From here's it shouldn't be too hard for anyone to use this information to figure out what to do on the server.  After all the logic is in place, the only thing we have left to do is to return the data, which isn't really that big deal at all.  The return data is basically plain text, but you can definitely send JSON back if you like.  If you would like to use JSON, you can even the `DataContractJsonSerializer` to serialize an object to the ASP.NET `Request.OutputStream` object:
	
	Object r = GetSomething(o);
	s.WriteObject(context.Response.OutputStream, r);

What about a more high-level approach that will allow me to simply write my core functionality without messing with mechanics?  Anyone using ASP.NET AJAX has this already in both their ASMX and WCF/JSON abstraction, but I wanted this functionality for Dojo (and for direct AJAX access).  My requirements were that I wanted to be able to define an attributed service, register it and move on.  Therefore, I build a Dojo RPC .NET 2.0 library called Dojr.NET (short for Dojo RPC, of course).  Dojr is probably the worst project name I've come up with to date, but it saves me from potential legal stuff from the Dojo Foundation.

## Using Dojr.NET

To use Dojr.NET, create a class that inherits from `DojoRpcServiceBase` and apply attribute `DojoOperationAttribute` on each publicly exposed method.  Be sure to also set the dojo.rpc operation name in it's constructor, this is the name the Dojo client will see.  Since .NET uses PascalCased methods and JavaScript uses camelCased function, this is required.  Here is a complete sample class:

	namespace NetFX.Web
	{
	    public class CalculatorService : DojoRpcServiceBase
	    {
	        [DojoOperation("add")]
	        public Int32 Add(Int32 n1, Int32 n2) {
	            return n1 + n2;
	        }
	
	        [DojoOperation("subtract")]
	        public Int32 Subtract(Int32 n1, Int32 n2) {
	            return n1 - n2;
	        }
	    }
	}

After this, all you have to do is register the class as an HttpHandler in your web.config file.

	<add verb="*" path="*/json/time/*" type="NetFX.Web.TimeService" />

At this point our Dojr.NET service is up and running, but how do we call it?  Actually, the same way you always do with dojo.rpc; nothing changes.  Believe it or not, this is a complete functional example:

	var calcProxy = newdojo.rpc.JsonService('json/calc/?smd');
	calcProxy.add(2, 3).addCallback(function(r) { alert(r); });

##Automatic Service Method Description
But, how did the proxy obtain the required dojo.rpc metadata?  If you look closely at the address given to the proxy you will notice that it's suffixed with `?smd`.  When a Dojr.NET service is suffixed with `?smd`, it will automatically generate and return the service metadata.  This is similar to putting `?wsdl` at the end of an ASMX URI.

Take a look at the metadata that's being automatically generated on the server via the ?smd suffix:
	
	{
	  "methods":[
	    {
	      "name":"add",
	      "parameters":[
	        {"name":"n1"},
	        {"name":"n2"}
	      ]
	    },
	    {
	      "name":"subtract",
	      "parameters":[
	        {"name":"n1"},
	        {"name":"n2"}
	      ]
	    }
	  ],
	  "serviceType":"JSON-RPC",
	  "serviceURL":"http://localhost:3135/Dojo/json/calc/"
	}

As you can see, Dojr.NET provides all the metadata required.  Literally all you have to do is inherit from `DojoRpcServiceBase`, apply the `DojoOperationAttribute`, and register the class to ASP.NET.  Everything else will be done for you.
