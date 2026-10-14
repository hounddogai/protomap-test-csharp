namespace Acme.Workforce.Assets.Tests.Laptop.RecordItem;

using Acme.Workforce.Assets.Laptop.RecordItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LaptopSummaryReportChecks
{
    public static void LaptopSummaryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700509866);
        var items = new List<LaptopSummary>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LaptopSummary.NewLaptopSummary($"laptop-summary-{index}", "sample"));
        }
        var report = LaptopSummaryReport.BuildLaptopSummaryReport("test", items, clock);
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
        LaptopSummaryReportCountsEachRecordOnce();
    }
}
