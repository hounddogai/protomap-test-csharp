namespace Acme.Workforce.Compensation.Tests.Benchmark.Entry;

using Acme.Workforce.Compensation.Benchmark.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class BenchmarkEntryChecks
{
    public static void BenchmarkEntryStartsInPlanned()
    {
        var item = BenchmarkEntry.NewBenchmarkEntry("code-909", "title-994");
        Check.Equal(item.Status, BenchmarkEntryStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BenchmarkEntryMovesToScheduled()
    {
        var item = BenchmarkEntry.NewBenchmarkEntry("code-909", "title-994");
        try
        {
            item.MoveTo(BenchmarkEntryStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BenchmarkEntryStatus.Scheduled, "status");
    }

    public static void BenchmarkEntryRejectsMovesFromCancelled()
    {
        var item = BenchmarkEntry.NewBenchmarkEntry("code-909", "title-994");
        item.Status = BenchmarkEntryStatus.Cancelled;
        Check.Fails(() => item.MoveTo(BenchmarkEntryStatus.Planned), "move");
    }

    public static void RunAll()
    {
        BenchmarkEntryStartsInPlanned();
        BenchmarkEntryMovesToScheduled();
        BenchmarkEntryRejectsMovesFromCancelled();
    }
}
