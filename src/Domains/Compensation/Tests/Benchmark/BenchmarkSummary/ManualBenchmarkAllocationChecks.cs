namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkSummary;

using Acme.Workforce.Compensation.Benchmark.BenchmarkSummary;

/// <summary>Checks of benchmark summary code, run by the project's runner.</summary>
public static class ManualBenchmarkAllocationChecks
{
    public static void ManualBenchmarkAllocationStartsInDraft()
    {
        var item = ManualBenchmarkAllocation.NewManualBenchmarkAllocation("code-36", "label-351");
        Check.Equal(item.Status, ManualBenchmarkAllocationStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualBenchmarkAllocationMovesToPending()
    {
        var item = ManualBenchmarkAllocation.NewManualBenchmarkAllocation("code-36", "label-351");
        try
        {
            item.MoveTo(ManualBenchmarkAllocationStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualBenchmarkAllocationStatus.Pending, "status");
    }

    public static void ManualBenchmarkAllocationRejectsMovesFromClosed()
    {
        var item = ManualBenchmarkAllocation.NewManualBenchmarkAllocation("code-36", "label-351");
        item.Status = ManualBenchmarkAllocationStatus.Closed;
        Check.Fails(() => item.MoveTo(ManualBenchmarkAllocationStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ManualBenchmarkAllocationStartsInDraft();
        ManualBenchmarkAllocationMovesToPending();
        ManualBenchmarkAllocationRejectsMovesFromClosed();
    }
}
