# Kody Terminal Payments DotNet Example
This example demonstrates how to use the `kody-dotnet8-client` library.
The library is available from: GitHub: https://nuget.pkg.github.com/KodyPay/index.json  
The latest library version can be found on this page: https://github.com/KodyPay/kody-clientsdk-dotnet/pkgs/nuget/kody-dotnet8-client

## dotnet client methods 
The methods available on the client are:
- `GetTerminals` - get the list of terminals for the store
- `SendPayment` - send a payment amount to a terminal
- `GetDetails` - get details about a payment
- `CancelPayment` - optionally cancel the payment request

The client needs to be created with the following properties:
- server address, the example uses the 'test' server.
- store ID - a Guid/UUID for a KodyPay store
- API Key - an API key associated with the store

## Example projects
`Demo-List-Terminals.cs` file contains code that reads the list of payment terminals assigned to the store.

The `Demo-Terminal-Payment.cs` code initiates a payment on a specific terminal, and checks the payment details.

## Demo
https://github.com/user-attachments/assets/ac32b325-dd2b-4562-8ad7-f67c76c27309
