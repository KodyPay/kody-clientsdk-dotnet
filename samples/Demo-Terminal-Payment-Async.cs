using Com.Kodypay.Grpc.Pay.V1;
using Grpc.Core;
using Grpc.Net.Client;

namespace kody_dotnet_samples;

public class KodyTerminalClient
{
    private const double RequestTimeoutMins = 3D; // 3 minute request timeout
    private readonly string _store;
    private readonly string _apiKey;
    private readonly KodyPayTerminalService.KodyPayTerminalServiceClient _client;

    /// <summary>
    /// Creates a new instance of the KodyPayTerminalClient class.
    /// </summary>
    /// <param name="address">The URI of the KodyPayTerminalService.</param>
    /// <param name="store">The unique identifier of the store.</param>
    /// <param name="apiKey">The API key for authentication.</param>
    public KodyPayTerminalClient(Uri address, Guid store, string apiKey)
    {
        _store = store.ToString();
        _apiKey = apiKey;
        _client = new KodyPayTerminalService.KodyPayTerminalServiceClient(GrpcChannel.ForAddress(address));
    }

    /// <summary>
    /// Sends a payment request with the specified amount and terminal ID.
    /// </summary>
    /// <param name="amount">The amount of the payment.</param>
    /// <param name="terminalId">The terminal ID.</param>
    /// <param name="orderIdCallback">An optional callback function to be called with the generated order ID.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the payment response object.</returns>
    public async Task<PayResponse> SendPayment(decimal amount, string terminalId, Action<string>? orderIdCallback = null)
    {
        var req = new PayRequest { StoreId = _store, Amount = amount.ToString("F2"), TerminalId = terminalId };
        using var pay = _client.Pay(req, deadline: DeadLine(), headers: ApiKey());
        var response = new PayResponse { Status = PaymentStatus.Pending };
        await foreach(var reply in pay.ResponseStream.ReadAllAsync())
        {
            if (reply.Status == PaymentStatus.Pending)
            {
                response = reply;
                orderIdCallback?.Invoke(response.OrderId);
            }
            else
            {
                response = reply;
                break;
            }
        }

        return response;
    }

    /// <summary>
    /// Cancels a payment with the specified amount, terminal ID, and order ID.
    /// </summary>
    /// <param name="amount">The amount of the payment to cancel.</param>
    /// <param name="terminalId">The ID of the terminal associated with the payment.</param>
    /// <param name="orderId">The ID of the order associated with the payment.</param>
    /// <returns>
    /// A <see cref="PaymentStatus"/> indicating the status of the cancellation request.
    /// </returns>
    public async Task<PaymentStatus> CancelPayment(decimal amount, string terminalId, string orderId)
    {
        var cancel = new CancelRequest
        {
            StoreId = _store,
            Amount = amount.ToString("F2"),
            TerminalId = terminalId,
            OrderId = orderId
        };
        var response = await _client.CancelAsync(cancel, deadline: DeadLine(), headers: ApiKey());
        return response.Status;
    }

    /// <summary>
    /// Retrieves a list of payment terminals.
    /// </summary>
    /// <returns>A list of payment terminals.</returns>
    public async Task<List<Terminal>> GetTerminals()
    {
        var request = new TerminalsRequest { StoreId = _store };
        var response = await _client.TerminalsAsync(request, headers: ApiKey());
        return response.Terminals.ToList();
    }

    /// <summary>
    /// Retrieves payment details for a given order ID.
    /// </summary>
    /// <param name="orderId">The ID of the order associated with the payment.</param>
    /// <returns>The payment response containing the payment details.</returns>
    public async Task<PayResponse> GetDetails(string orderId)
    {
        var details = new PaymentDetailsRequest
        {
            StoreId = _store,
            OrderId = orderId
        };
        return await _client.PaymentDetailsAsync(details, deadline: DeadLine(), headers: ApiKey());
    }

    private static DateTime DeadLine()
    {
        return DateTime.UtcNow.AddMinutes(RequestTimeoutMins);
    }
    private Metadata ApiKey()
    {
        return new Metadata { { "X-API-Key", _apiKey } };
    }
}
