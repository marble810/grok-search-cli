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
    Environment.Exit(1);
}

static async Task MainInner(string[] args, TextWriter stderr)
{
    var tool = CliLogic.ParseTool(args);
    var query = ReadQuery(args);
    var filters = CliLogic.ParseFilters(args);

    var apiKey = Credentials.Resolve(stderr);
    var client = new XaiClient(apiKey);

    var xaiRequest = new XaiRequest
    {
        Model = CliLogic.Model,
        Input = query,
        Tools = CliLogic.BuildTools(tool, filters)
    };

    var response = await client.SearchAsync(xaiRequest, stderr);

    var output = CliLogic.BuildOutput(tool, response);
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
