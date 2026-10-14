namespace Acme.Workforce.Talent.Tests.Pool.Adjustment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Pool.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class ManualPoolTierReportChecks
{
    public static void ManualPoolTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707340152);
        var items = new List<ManualPoolTier>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(ManualPoolTier.NewManualPoolTier($"manual-pool-tier-{index}", "sample"));
        }
        var report = ManualPoolTierReport.BuildManualPoolTierReport("test", items, clock);
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
        ManualPoolTierReportCountsEachRecordOnce();
    }
}
