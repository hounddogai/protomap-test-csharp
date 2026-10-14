namespace Acme.Workforce.Compensation.Tests.Benchmark.Rollup;

using Acme.Workforce.Compensation.Benchmark.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class ManualBenchmarkWindowChecks
{
    public static void ManualBenchmarkWindowStartsInDraft()
    {
        var item = ManualBenchmarkWindow.NewManualBenchmarkWindow("code-892", "title-712");
        Check.Equal(item.Status, ManualBenchmarkWindowStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualBenchmarkWindowMovesToPending()
    {
        var item = ManualBenchmarkWindow.NewManualBenchmarkWindow("code-892", "title-712");
        try
        {
            item.MoveTo(ManualBenchmarkWindowStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualBenchmarkWindowStatus.Pending, "status");
    }

    public static void ManualBenchmarkWindowRejectsMovesFromClosed()
    {
        var item = ManualBenchmarkWindow.NewManualBenchmarkWindow("code-892", "title-712");
        item.Status = ManualBenchmarkWindowStatus.Closed;
        Check.Fails(() => item.MoveTo(ManualBenchmarkWindowStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ManualBenchmarkWindowStartsInDraft();
        ManualBenchmarkWindowMovesToPending();
        ManualBenchmarkWindowRejectsMovesFromClosed();
    }
}
