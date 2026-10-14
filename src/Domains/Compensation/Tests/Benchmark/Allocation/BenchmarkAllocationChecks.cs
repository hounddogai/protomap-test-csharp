namespace Acme.Workforce.Compensation.Tests.Benchmark.Allocation;

using Acme.Workforce.Compensation.Benchmark.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class BenchmarkAllocationChecks
{
    public static void BenchmarkAllocationStartsInOpen()
    {
        var item = BenchmarkAllocation.NewBenchmarkAllocation("code-50", "label-320");
        Check.Equal(item.Status, BenchmarkAllocationStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BenchmarkAllocationMovesToArchived()
    {
        var item = BenchmarkAllocation.NewBenchmarkAllocation("code-50", "label-320");
        try
        {
            item.MoveTo(BenchmarkAllocationStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BenchmarkAllocationStatus.Archived, "status");
    }

    public static void BenchmarkAllocationRejectsMovesFromArchived()
    {
        var item = BenchmarkAllocation.NewBenchmarkAllocation("code-50", "label-320");
        item.Status = BenchmarkAllocationStatus.Archived;
        Check.Fails(() => item.MoveTo(BenchmarkAllocationStatus.Open), "move");
    }

    public static void RunAll()
    {
        BenchmarkAllocationStartsInOpen();
        BenchmarkAllocationMovesToArchived();
        BenchmarkAllocationRejectsMovesFromArchived();
    }
}
