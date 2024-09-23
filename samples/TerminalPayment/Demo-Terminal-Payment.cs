using System.Diagnostics;
using System.Text.Json;
using Com.Kodypay.Grpc.Pay.V1;
using CommandLine;
using KodyCommons;

namespace kody_dotnet_samples;



public class ExamplePayment
{
    public class PaymentOptions
    {
        [Option('t', "terminalId", Required = true, HelpText = "Terminal ID")]
        public string TerminalId { get; set; }

        [Option('a', "amount", Default = "1.00", HelpText = "Amount to charge")]
        public string Amount { get; set; }

        [Option('s', "showTips", Required = false, Default = false, HelpText = "Show tips")]
        public bool ShowTips { get; set; }
    }
    
    public static void Main(string[] args)
    {
        // setup the configuration needed by the client
        var settings = Utils.LoadSettings("appsettings.json");
        Debug.Assert(settings != null, $"{nameof(settings)} != null");

        Parser.Default.ParseArguments<PaymentOptions>(args)
            .WithParsed<PaymentOptions>(opts =>
            {
                var example = new ExamplePayment(settings, opts);
                example.Pay();
                example.Details();
            })
            .WithNotParsed<PaymentOptions>((errs) => HandleParseError(errs));
    }

    private readonly KodyPayTerminalClient _client;

    // sample data
    private decimal _amount;
    private string _terminalId;
    private string _currentOrderId = "";
    private bool _showTips = false;

    private ExamplePayment(KodySettings settings, PaymentOptions opts)
    {
        // the API Key and Store will be given to you when you start developing the integration.
        var store = Guid.Parse(settings.StoreId);
        var address = new Uri(settings.Address);
        // the client can be created once and reused
        _client = new KodyPayTerminalClient(address, store, settings.ApiKey);
        
        _terminalId = opts.TerminalId;
        _amount = Convert.ToDecimal(opts.Amount);
        _showTips = opts.ShowTips;
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
                // the terminal ID, retrieved by the GetTerminals function (above)
                // the amount to pay, specified as a decimal
                terminalId: _terminalId,
                amount: _amount,
                showTips: _showTips,
                // this argument is an optional callback to save the order ID locally to be used by the CancelPayment function
                orderIdCallback: orderId =>
                {
                    var timestamp2 = DateTime.UtcNow.ToString("u");
                    Console.WriteLine($"[{timestamp2}] Processing payment with ID: {orderId}");
                    _currentOrderId = orderId;
                    // By default the payment cancels in 2 minutes. if you want a shorter timeout, you need to cancel the payment yourself
                    Utils.StartCountdown("Waiting {0} seconds for payment...", 30);
                    Cancel();
                })
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
    
    public static void HandleParseError(IEnumerable<Error> errs)
    {
        // Handle command line parsing errors
        Console.WriteLine("Error parsing arguments.");
    }
}
