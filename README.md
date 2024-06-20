# Kody .NET gRPC Client

## Description
The Kody .NET gRPC Client is an SDK generated from protobuf protocols to facilitate communication with the Kody Payments Gateway. This library provides a simple and efficient way to integrate Kody payment functionalities into your .NET applications.

## Requirements
- .NET 8.0 or later (if you need another version contact Kody)

## Installation

### Step 1: Install via NuGet
To install the Kody .NET gRPC Client, you can add it to your project's `.csproj` file and run `dotnet restore`.

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
      <PackageReference Include="kody-dotnet8-client" Version="240620.x" />
  </ItemGroup>

</Project>
```

## Usage

On the below diagram, the code that needs to be implemented on your side is the Payment Code in green. 
The Kody .NET SDK in blue is the library provided by this package, and makes the network communications to Kody platform directly.
![Kody POS Integration](https://github.com/KodyPay/project-bamboo/assets/103110620/dd3939ab-6e03-424c-8e1e-9bfa3b89393f)

### Example Code
Here is an example of how to use the Kody .NET gRPC client to communicate with the Kody Payments Gateway:

```csharp
using System;
using Grpc.Core;
using Grpc.Net.Client;
using Com.Kodypay.Grpc.Pay.V1;

class Program
{
    static void Main(string[] args)
    {
        const string Host = "grpc.kodypay.com";
        const string StoreId = "5fa2dd05-1805-494d-b843-fa1a7c34cf8a"; // Use your Kody store ID
        const string ApiKey = "YOUR_API_KEY"; // Put your API key

        var channel = GrpcChannel.ForAddress($"https://{Host}");
        var client = new KodyPayTerminalService.KodyPayTerminalServiceClient(channel);

        var headers = new Metadata
        {
            { "X-API-Key", ApiKey }
        };

        Console.WriteLine("Requesting the list of terminals assigned to the store");

        var request = new TerminalsRequest { StoreId = StoreId };
        var response = client.Terminals(request, headers);

        Console.WriteLine($"Terminals for Store ID: {StoreId}");
        foreach (var terminal in response.Terminals)
        {
            Console.WriteLine($"Terminal ID: {terminal.TerminalId} - Online: {(terminal.Online ? "Yes" : "No")}");
        }
    }
}
```

### More examples
There are more examples on the `samples` folder of this repository, including how to make payments on a terminal device and check transaction details.

## Troubleshooting
If you encounter issues, ensure:
- All required .NET packages are installed.
- Your `.csproj` file is correctly set up and all dependencies are installed.
- The gRPC server you are trying to connect to is running and accessible.
- Contact Kody support or the tech team if you need further assistance.

## License
This project is licensed under the MIT License.
