namespace Acme.Workforce.Payrollops.Tests.RetroPay.Checkpoint;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.RetroPay.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class RegionalRetroPayTierReportChecks
{
    public static void RegionalRetroPayTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703545818);
        var items = new List<RegionalRetroPayTier>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(RegionalRetroPayTier.NewRegionalRetroPayTier($"regional-retro-pay-tier-{index}", "sample"));
        }
        var report = RegionalRetroPayTierReport.BuildRegionalRetroPayTierReport("test", items, clock);
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
        RegionalRetroPayTierReportCountsEachRecordOnce();
    }
}
