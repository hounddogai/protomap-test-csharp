namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkEntry;

using Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;

/// <summary>Checks of benchmark entry code, run by the project's runner.</summary>
public static class ScheduledBenchmarkEntryChecks
{
    public static void ScheduledBenchmarkEntryStartsInRequested()
    {
        var item = ScheduledBenchmarkEntry.NewScheduledBenchmarkEntry("code-545", "label-198");
        Check.Equal(item.Status, ScheduledBenchmarkEntryStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledBenchmarkEntryMovesToExpired()
    {
        var item = ScheduledBenchmarkEntry.NewScheduledBenchmarkEntry("code-545", "label-198");
        try
        {
            item.MoveTo(ScheduledBenchmarkEntryStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledBenchmarkEntryStatus.Expired, "status");
    }

    public static void ScheduledBenchmarkEntryRejectsMovesFromExpired()
    {
        var item = ScheduledBenchmarkEntry.NewScheduledBenchmarkEntry("code-545", "label-198");
        item.Status = ScheduledBenchmarkEntryStatus.Expired;
        Check.Fails(() => item.MoveTo(ScheduledBenchmarkEntryStatus.Requested), "move");
    }

    public static void RunAll()
    {
        ScheduledBenchmarkEntryStartsInRequested();
        ScheduledBenchmarkEntryMovesToExpired();
        ScheduledBenchmarkEntryRejectsMovesFromExpired();
    }
}
