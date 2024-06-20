using System.Diagnostics;
using System.Text.Json;
using Com.Kodypay.Grpc.Pay.V1;
using kp_bamboo_dotnet;

namespace kody_dotnet_samples;

public record KodySettings(string Address, string StoreId, string ApiKey);

public class ExamplePayment
{
    public static void Main()
    {
        // setup the configuration needed by the client
        var settings = JsonSerializer.Deserialize<ExampleSettings> (File.ReadAllText("../../../appsettings.json"));
        Debug.Assert(settings != null, $"{nameof(settings)} != null");

        var example = new Example(settings)
        {
            _terminalId = "S1F2-000158213604086"
        };
        example.Pay();
        example.Details();
    }

    private readonly KodyPayTerminalClient _client;

    // sample data
    private readonly decimal _amount = Convert.ToDecimal("1.00");
    private string _terminalId = "";
    private string _currentOrderId = "";

    private Example(ExampleSettings settings)
    {
        // the API Key and Store will be given to you when you start developing the integration.
        var store = Guid.Parse(settings.StoreId);
        var address = new Uri(settings.Address);
        // the client can be created once and reused
        _client = new KodyPayTerminalClient(address, store, settings.ApiKey);
    }

    private void Pay()
    {
        var timestamp = DateTime.UtcNow.ToString("u");
        Console.WriteLine($"[{timestamp}] Sending payment for amount: {_amount} to terminal: {_terminalId}");
        _currentOrderId = "";

        // this function can be used to send a payment:
        var payment = Task.Run(async () =>
            // the SendPayment function expects 2 or 3 arguments:
            await _client.SendPayment(
                // the amount to pay, specified as a decimal
                amount: _amount,
                // the terminal ID, retrieved by the GetTerminals function (above)
                // in this example it is assumed that there is at least one terminal, the first is selected
                terminalId: _terminalId,

                // this argument is an optional callback to save the order ID locally to be used by the CancelPayment function
                orderIdCallback: orderId =>
                {
                    var timestamp2 = DateTime.UtcNow.ToString("u");
                    Console.WriteLine($"[{timestamp2}] Processing payment with ID: {orderId}");
                    _currentOrderId = orderId;
                    // Console.WriteLine("Sleeping...");
                    // Thread.Sleep(TimeSpan.FromSeconds(10));
                    // Cancel();
                }
            )
        ).Result;
        // the payment result object includes the following properties:
        // payment.Status        // (Pending, Success, Cancelled, Failed)
        // payment.OrderId       // internal 'order' ID generated for the payment
        // payment.DateCreated   // when the payment request was received by the server
        // payment.DatePaid      // when the payment completed on the terminal (if status is Success)
        // payment.FailureReason // reason for failure, only populated when status is Cancelled or Failed
        // payment.ExtPaymentRef // external payment reference, PSP reference
        // payment.ReceiptJson   // JSON object containing the receipt data

        var timestamp3 = DateTime.UtcNow.ToString("u");
        Console.WriteLine($"[{timestamp3}] Payment complete: {payment}");
        if (payment.Status == PaymentStatus.Pending)
        {
            Console.WriteLine($"[{timestamp3}] Payment still pending, cancelling: {payment}");
            Cancel();
        }
    }
    
    private void Cancel()
    {
        Console.WriteLine($"Cancelling payment: {_currentOrderId}");
        // this additional function can be used to cancel an ongoing payment:
        var paymentStatus = Task.Run(async () =>
            // the CancelPayment function expects 3 arguments:
            // the amount paid, the terminal ID, the payment 'order' ID generated when the payment is sent
            await _client.CancelPayment(_amount, _terminalId, _currentOrderId)
        ).Result;
    
        Console.WriteLine($"Payment cancelled: {paymentStatus}");
    
        // paymentStatus: the cancel result contains the payment status, Cancelled should be expected.
        // (Pending, Success, Cancelled, Failed)
    }

    private void Details()
    {
        var timestamp4 = DateTime.UtcNow.ToString("u");
        Console.WriteLine($"[{timestamp4}] Get payment details: {_currentOrderId}");
        // this function can be used to get the details of a completed payment:
        var paymentDetails = Task.Run(async () =>
            await _client.GetDetails(_currentOrderId)
        ).Result;
        
        Console.WriteLine($"Payment details: {paymentDetails}");
        // the paymentDetails result object includes the following properties (same information as payment result):
        // payment.Status        // (Pending, Success, Cancelled, Failed)
        // payment.OrderId       // internal 'order' ID generated for the payment
        // payment.DateCreated   // when the payment request was received by the server
        // payment.DatePaid      // when the payment completed on the terminal (if status is Success)
        // payment.FailureReason // reason for failure, only populated when status is Cancelled or Failed
        // payment.ExtPaymentRef // external payment reference, PSP reference
        // payment.ReceiptJson   // JSON object containing the receipt data
    }
}
