# Kody .NET gRPC Client

## Description
The Kody .NET gRPC Client is an SDK generated from protobuf protocols, designed to streamline communication with the 
Kody Integration Gateway. This library offers a straightforward and efficient way to integrate Kody’s functionalities 
into your .NET applications.

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
      <PackageReference Include="kody-dotnet8-client" Version="1.4.24-alpha" />
  </ItemGroup>

</Project>
```

## Usage

On the below diagram, the code that needs to be implemented on your side is the integration Code in green. 
The Kody .NET SDK in blue is the library provided by this package, and makes the network communications to Kody platform directly.
![Kody POS Integration](https://github.com/KodyPay/project-bamboo/assets/103110620/dd3939ab-6e03-424c-8e1e-9bfa3b89393f)

### Example Code
Here is an example of how to use the Kody .NET gRPC client to communicate with the Kody Integration Gateway for Payments and Ordering:

```csharp
using System;
using Grpc.Core;
using Grpc.Net.Client;
using Com.Kodypay.Grpc.Pay.V1;
using Com.Kodypay.Grpc.Ordering.V1;
using static Com.Kodypay.Grpc.Ordering.V1.Order.Types;

class Program
{
    private const string Host = "grpc.kodypay.com";
    private const string StoreId = "5fa2dd05-1805-494d-b843-fa1a7c34cf8a"; // Use your Kody store ID
    private const string ApiKey = "YOUR_API_KEY"; // Put your API key

    static void Main(string[] args)
    {
        var channel = GrpcChannel.ForAddress($"https://{Host}");
        var headers = new Metadata
        {
            { "X-API-Key", ApiKey }
        };
        
        GetTerminals(StoreId, channel, headers);
        GetOrders(StoreId, channel, headers);
        UpdateOrderStatus("Completed", channel, headers);
        GetInventory(StoreId, channel, headers);
    }

    private static void GetTerminals(string StoreId, GrpcChannel channel, Metadata headers)
    {
        Console.WriteLine("Requesting the list of terminals assigned to the store");
        var client = new KodyPayTerminalService.KodyPayTerminalServiceClient(channel);
        var request = new TerminalsRequest { StoreId = StoreId };
        var response = client.Terminals(request, headers);

        Console.WriteLine($"Terminals for Store ID: {StoreId}");
        foreach (var terminal in response.Terminals)
        {
            Console.WriteLine($"Terminal ID: {terminal.TerminalId} - Online: {(terminal.Online ? "Yes" : "No")}");
        }
    }

    private static void GetOrders(string StoreId, GrpcChannel channel, Metadata headers)
    {
        Console.WriteLine("Requesting the list of orders assigned to the store");
        var client = new OrderService.OrderServiceClient(channel);
        var request = new GetOrdersRequest { StoreId = StoreId };
        var response = client.GetOrders(request, headers);

        Console.WriteLine($"Orders for Store ID: {StoreId}");
        foreach (var order in response.Orders)
        {
            Console.WriteLine($"Order ID: {order.OrderId}, Status: {order.Status}");
            foreach (var itemOrCombo in order.Items)
            {
                if (itemOrCombo.TypeCase == OrderItemOrCombo.TypeOneofCase.Item)
                {
                    var item = itemOrCombo.Item;
                    Console.WriteLine($"  Item: {item.ItemId}, Quantity: {item.Quantity}");
                }
                else if (itemOrCombo.TypeCase == OrderItemOrCombo.TypeOneofCase.Combo)
                {
                    var combo = itemOrCombo.Combo;
                    Console.WriteLine($"  Combo: {combo.ComboId}, Quantity: {combo.Quantity}");
                }
            }
        }
    }

    private static void UpdateOrderStatus(string Status, GrpcChannel channel, Metadata headers)
    {
        var client = new OrderService.OrderServiceClient(channel);
        var request = new UpdateOrderStatusRequest {
            StoreId = "store123",
            OrderId = "order456",
            NewStatus = Enum.Parse<OrderStatus>(Status, true)
        };

        var response = client.UpdateOrderStatus(request, headers);

        Console.WriteLine($"Order ID: {"order456"} updated to status: {Status}");
    }

    private static void GetInventory(string StoreId, GrpcChannel channel, Metadata headers)
    {
        var client = new InventoryService.InventoryServiceClient(channel);
        var request = new GetInventoryRequest { StoreId = StoreId };
        var response = client.GetInventory(request, headers);

        Console.WriteLine($"Inventory for Store ID: {StoreId}");
        foreach (var inventoryItemOrCombo in response.Items)
        {
            if (inventoryItemOrCombo.TypeCase == InventoryItemOrCombo.TypeOneofCase.Item)
            {
                var item = inventoryItemOrCombo.Item;
                Console.WriteLine($"  Item: {item.ItemId}, Name: {item.Name}");
            }
            else if (inventoryItemOrCombo.TypeCase == InventoryItemOrCombo.TypeOneofCase.Combo)
            {
                var combo = inventoryItemOrCombo.Combo;
                Console.WriteLine($"  Combo: {combo.ComboId}, Name: {combo.Name}");
            }
          
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
