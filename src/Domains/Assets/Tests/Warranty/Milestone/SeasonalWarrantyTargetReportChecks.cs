namespace Acme.Workforce.Assets.Tests.Warranty.Milestone;

using Acme.Workforce.Assets.Warranty.Milestone;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class SeasonalWarrantyTargetReportChecks
{
    public static void SeasonalWarrantyTargetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706252131);
        var items = new List<SeasonalWarrantyTarget>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(SeasonalWarrantyTarget.NewSeasonalWarrantyTarget($"seasonal-warranty-target-{index}", "sample"));
        }
        var report = SeasonalWarrantyTargetReport.BuildSeasonalWarrantyTargetReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        SeasonalWarrantyTargetReportCountsEachRecordOnce();
    }
}
