namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkEntry;

using Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;

/// <summary>Checks of benchmark entry code, run by the project's runner.</summary>
public static class ScheduledBenchmarkEntryViewChecks
{
    public static void ScheduledBenchmarkEntryViewShowsTheStatusInCapitals()
    {
        var item = ScheduledBenchmarkEntry.NewScheduledBenchmarkEntry("code-545", "label-198");
        var view = ScheduledBenchmarkEntryView.ToScheduledBenchmarkEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledBenchmarkEntryView.ToScheduledBenchmarkEntryViews(new List<ScheduledBenchmarkEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledBenchmarkEntryViewShowsTheStatusInCapitals();
    }
}
