using System.Diagnostics;
using KodyCommons;

namespace kody_dotnet_samples;

public class ExampleListTerminals
{
    public static void Main()
    {
        // setup the configuration needed by the client
        var settings = Utils.LoadSettings("appsettings.json");
        Debug.Assert(settings != null, $"{nameof(settings)} != null");

        var example = new ExampleListTerminals(settings);
        example.Terminals();
    }

    private readonly KodyPayTerminalClient _client;

    private ExampleListTerminals(KodySettings settings)
    {
        // the API Key and Store will be given to you when you start developing the integration.
        var store = Guid.Parse(settings.StoreId);
        var address = new Uri(settings.Address);
        // the client can be created once and reused
        _client = new KodyPayTerminalClient(address, store, settings.ApiKey);
    }

    private void Terminals()
    {
        // get a list of terminals assigned to the store, including whether they are online or offline
        var terminals = Task.Run(async () =>
            // the GetTerminals function expects no arguments, it uses the store configured in the client
            await _client.GetTerminals()
        ).Result;

        Console.WriteLine($"Found {terminals.Count} terminals");
        foreach (var terminal in terminals)
        {
            Console.WriteLine($"TerminalId: {terminal.TerminalId}, Online: {terminal.Online}");
        }

        var _terminalId = terminals.First(t => t.Online).TerminalId;
        Console.WriteLine($"Selected terminal: {_terminalId}");
    }
}
