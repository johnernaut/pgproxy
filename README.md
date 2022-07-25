# pgproxy

PGProxy lets you act as both a Postgres client *and* server so that you can do things like log incoming queries,
manipulate return data (such as obfuscating sensitive information before it gets back to the client), etc...

## Running

Currently `pgproxy` requires .NET 7 or later which you can
download  [here](https://dotnet.microsoft.com/en-us/download/dotnet/7.0). Once installed, you can run `dotnet run` in
your terminal from the root directory to fire up the proxy.

