# Dojr.NET Dojo RPC Library

Dojr.NET is a simple framework created to allow easy creation of dojo.rpc services by defining a class which inherits from a specific base class, adding one or more attributes, and registering it as an HttpHandler.

## Description

Dojr.NET is built on .NET 3.5 technology and attempts to give WCF-like usability. Dojr.NET actually internally uses DataContractJsonSerializer which WCF 3.5 internally uses for its own JSON serialization. Dojr.NET, like WCF, uses attributes to declare specific service operations. However, unlike WCF, Dojr.NET inherits from a class, like COM+, where as it is idiomatic in WCF to implement a service contract interface. Dojr.NET creates each service as an HttpHandler which is then registered to ASP.NET at a particular endpoint.
