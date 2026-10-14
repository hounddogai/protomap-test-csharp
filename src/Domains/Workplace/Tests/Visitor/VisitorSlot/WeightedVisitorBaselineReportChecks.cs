namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorSlot;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Visitor.VisitorSlot;

/// <summary>Checks of visitor slot code, run by the project's runner.</summary>
public static class WeightedVisitorBaselineReportChecks
{
    public static void WeightedVisitorBaselineReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700140829);
        var items = new List<WeightedVisitorBaseline>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(WeightedVisitorBaseline.NewWeightedVisitorBaseline($"weighted-visitor-baseline-{index}", "sample"));
        }
        var report = WeightedVisitorBaselineReport.BuildWeightedVisitorBaselineReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        WeightedVisitorBaselineReportCountsEachRecordOnce();
    }
}
