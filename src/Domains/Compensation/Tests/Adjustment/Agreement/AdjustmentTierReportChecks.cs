namespace Acme.Workforce.Compensation.Tests.Adjustment.Agreement;

using Acme.Workforce.Compensation.Adjustment.Agreement;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class AdjustmentTierReportChecks
{
    public static void AdjustmentTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706785661);
        var items = new List<AdjustmentTier>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(AdjustmentTier.NewAdjustmentTier($"adjustment-tier-{index}", "sample"));
        }
        var report = AdjustmentTierReport.BuildAdjustmentTierReport("test", items, clock);
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
        AdjustmentTierReportCountsEachRecordOnce();
    }
}
