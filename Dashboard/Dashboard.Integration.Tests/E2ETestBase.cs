namespace Dashboard.Integration.Tests;

/// <summary>
/// Base class for every E2E test class. Uses xUnit's per-test
/// <see cref="IAsyncLifetime"/> hook to reset the shared fixture's databases
/// to a clean-slate baseline before each test runs, guaranteeing that no test
/// inherits state from any other test.
/// </summary>
public abstract class E2ETestBase : IAsyncLifetime
{
    /// <summary>Shared E2E fixture (one infrastructure stack for the whole run).</summary>
    protected E2EFixture Fixture { get; }

    /// <summary>Base class stores the shared fixture for derived tests to use.</summary>
    protected E2ETestBase(E2EFixture fixture) => Fixture = fixture;

    /// <summary>
    /// Runs before every single test. Truncates all data tables in the
    /// Clinical, Scheduling, and Gatekeeper databases (restarting identity
    /// sequences) and re-seeds the baseline patient, practitioners, and
    /// appointment so every test starts from the exact same state.
    /// </summary>
    public Task InitializeAsync() => Fixture.ResetAsync();

    /// <summary>
    /// No per-test teardown required — the next test's <see cref="InitializeAsync"/>
    /// will truncate and re-seed.
    /// </summary>
    public Task DisposeAsync() => Task.CompletedTask;
}
