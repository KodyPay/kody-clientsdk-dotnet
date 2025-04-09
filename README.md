# Kody API – .NET SDK

This guide provides an overview of using the Kody .NET gRPC Client SDK and its reference documentation.

- [Client Libraries](#client-libraries)
- [.NET Installation](#net-installation)
- [Authentication](#authentication)
- [Documentation](#documentation)
- [Sample Code](#sample-code)

## Client Libraries

Kody provides client libraries for many popular languages to access the APIs. If your desired programming language is supported by the client libraries, we recommend that you use this option.

Available languages:
- .NET: https://github.com/KodyPay/kody-clientsdk-dotnet/
- Java: https://github.com/KodyPay/kody-clientsdk-java/
- Python: https://github.com/KodyPay/kody-clientsdk-python/
- PHP: https://github.com/KodyPay/kody-clientsdk-php/

The advantages of using the Kody Client library instead of a REST API are:
- Maintained by Kody.
- Built-in authentication and increased security.
- Built-in retries.
- Idiomatic for each language.
- Quicker development.
- Backwards compatibility with new versions.

If your coding language is not listed, please let the Kody team know and we will be able to create it for you.

## .NET Installation

### Requirements

- .NET 8.0 or later
  *(Contact the Kody team if support for another version is needed)*

### Step 1: Install via NuGet

Add the SDK to your `.csproj` file:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="kody-dotnet8-client" Version="1.6.3" />
  </ItemGroup>

</Project>
```

Then restore the dependencies:

```bash
dotnet restore
```

## Authentication

The client library uses a combination of a `Store ID` and an `API key`.

These credentials will be provided during onboarding. You will receive a **test Store ID** and **API key** for development, and **live credentials** for production access.

### Host names

- Development and test: `https://grpc-staging.kodypay.com`
- Live: `https://grpc.kodypay.com`

## Documentation

For complete API documentation, examples, and integration guides, please visit:
📚 https://api-docs.kody.com

## Sample Code

### Example – Get Terminals, Orders, Inventory and Update Order Status

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
    private const string StoreId = "YOUR_STORE_ID"; // Use your Kody store ID
    private const string ApiKey = "YOUR_API_KEY"; // Use your API key

    static void Main(string[] args)
    {
        var channel = GrpcChannel.ForAddress($"https://{Host}");
        var headers = new Metadata { { "X-API-Key", ApiKey } };

        GetTerminals(StoreId, channel, headers);
        GetOrders(StoreId, channel, headers);
        UpdateOrderStatus("Completed", channel, headers);
        GetInventory(StoreId, channel, headers);
    }

    private static void GetTerminals(string storeId, GrpcChannel channel, Metadata headers)
    {
        var client = new KodyPayTerminalService.KodyPayTerminalServiceClient(channel);
        var request = new TerminalsRequest { StoreId = storeId };
        var response = client.Terminals(request, headers);

        Console.WriteLine($"Terminals for Store ID: {storeId}");
        foreach (var terminal in response.Terminals)
        {
            Console.WriteLine($"Terminal ID: {terminal.TerminalId} - Online: {(terminal.Online ? "Yes" : "No")}");
        }
    }

    private static void GetOrders(string storeId, GrpcChannel channel, Metadata headers)
    {
        var client = new OrderService.OrderServiceClient(channel);
        var request = new GetOrdersRequest { StoreId = storeId };
        var response = client.GetOrders(request, headers);

        Console.WriteLine($"Orders for Store ID: {storeId}");
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

    private static void UpdateOrderStatus(string status, GrpcChannel channel, Metadata headers)
    {
        var client = new OrderService.OrderServiceClient(channel);
        var request = new UpdateOrderStatusRequest
        {
            StoreId = "store123",
            OrderId = "order456",
            NewStatus = Enum.Parse<OrderStatus>(status, true)
        };

        var response = client.UpdateOrderStatus(request, headers);
        Console.WriteLine($"Order ID: {"order456"} updated to status: {status}");
    }

    private static void GetInventory(string storeId, GrpcChannel channel, Metadata headers)
    {
        var client = new InventoryService.InventoryServiceClient(channel);
        var request = new GetInventoryRequest { StoreId = storeId };
        var response = client.GetInventory(request, headers);

        Console.WriteLine($"Inventory for Store ID: {storeId}");
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

## Sample Code Repositories

- .NET: https://github.com/KodyPay/kody-clientsdk-dotnet/tree/main/samples
- Java: https://github.com/KodyPay/kody-clientsdk-java/tree/main/samples
- Python: https://github.com/KodyPay/kody-clientsdk-python/tree/main/versions/3_12/samples
- PHP: https://github.com/KodyPay/kody-clientsdk-php/tree/main/samples

## Troubleshooting

If you encounter issues, ensure:
- All required NuGet packages are installed.
- Your `.csproj` file is correctly set up.
- The gRPC server is reachable.
- Contact Kody support or the tech team if further assistance is needed.

## License

This project is licensed under the MIT License.
