using XaiSearchCli;
using XaiSearchCli.Clients;
using XaiSearchCli.Configuration;
using XaiSearchCli.Models;

var stderr = Console.Error;

try
{
    await MainInner(args, stderr);
}
catch (CommandException ex)
{
    stderr.WriteLine($"error: {ex.Message}");
    Environment.Exit(ex.ExitCode);
}

static async Task MainInner(string[] args, TextWriter stderr)
{
    // Dispatch to discovery entrypoints before search flow
    if (args.Length > 0 && args[0] is "help" or "--help" or "-h")
    {
        DiscoveryCommand.HandleHelp(args[1..], stderr);
        return;
    }

    if (args.Length > 0 && args[0] == "describe")
    {
        DiscoveryCommand.HandleDescribe(stderr);
        return;
    }

    // Dispatch to auth command group when first argument is "auth"
    if (args.Length > 0 && args[0] == "auth")
    {
        AuthCommand.Handle(args[1..], stderr);
        return;
    }

    var tool = CliLogic.ParseTool(args);
    var query = ReadQuery(args);
    var model = CliLogic.ParseModel(args);
    var filters = CliLogic.ParseFilters(tool, args);

    var apiKey = Credentials.Resolve(stderr);
    var client = new XaiClient(apiKey);

    var xaiRequest = new XaiRequest
    {
        Model = model,
        Input = query,
        Tools = CliLogic.BuildTools(tool, filters)
    };

    var response = await client.SearchAsync(xaiRequest, stderr);

    var output = CliLogic.BuildOutput(tool, model, response);
    Console.Out.WriteLine(CliLogic.SerializeOutput(output));
}

static string ReadQuery(string[] args)
{
    var positional = CliLogic.GetPositionalArgs(args);
    bool hasStdin = StdinDetector.HasStdinInput;
    bool hasPositional = positional.Length > 0;

    if (hasPositional && hasStdin)
        throw new CommandException("provide the query as a positional argument OR via stdin, not both");

    if (hasPositional)
        return string.Join(" ", positional);

    if (hasStdin)
    {
        var input = Console.In.ReadToEnd();
        if (string.IsNullOrWhiteSpace(input))
            throw new CommandException("stdin was empty; provide a query string");
        return input.Trim();
    }

    throw new CommandException("provide a search query as a positional argument or via stdin");
}
