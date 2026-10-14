namespace Acme.Workforce.Workplace.Tests.Visitor.Entry;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Visitor.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class WeightedVisitorEntryReportChecks
{
    public static void WeightedVisitorEntryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708913680);
        var items = new List<WeightedVisitorEntry>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(WeightedVisitorEntry.NewWeightedVisitorEntry($"weighted-visitor-entry-{index}", "sample"));
        }
        var report = WeightedVisitorEntryReport.BuildWeightedVisitorEntryReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        WeightedVisitorEntryReportCountsEachRecordOnce();
    }
}
