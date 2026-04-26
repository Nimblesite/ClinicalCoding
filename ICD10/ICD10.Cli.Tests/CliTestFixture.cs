using ICD10.Api.Tests;

namespace ICD10.Cli.Tests;

/// <summary>
/// Test fixture that spins up a real API with seeded test data.
/// CLI tests run against this real API.
/// </summary>
public sealed class CliTestFixture : IDisposable
{
    private readonly ICD10ApiFactory _factory;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Gets the API base URL.
    /// </summary>
    public string ApiUrl { get; }

    /// <summary>
    /// Gets the HTTP client configured for the test API.
    /// </summary>
    public HttpClient HttpClient => _httpClient;

    /// <summary>
    /// Creates a new test fixture with a real API server.
    /// </summary>
    public CliTestFixture()
    {
        _factory = new ICD10ApiFactory();
        _httpClient = _factory.CreateClient();
        ApiUrl = _httpClient.BaseAddress?.ToString().TrimEnd('/') ?? "http://localhost";
    }

    /// <summary>
    /// Runs the CLI with the given input lines (each pushed with Enter) and returns the captured console output.
    /// </summary>
    public async Task<string> RunCliAsync(params string[] inputs)
    {
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = true;
        foreach (var input in inputs)
        {
            console.Input.PushTextWithEnter(input);
        }

        using var cli = new Icd10Cli(ApiUrl, console, HttpClient);
        await cli.RunAsync();
        return console.Output;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _httpClient.Dispose();
        _factory.Dispose();
    }
}
