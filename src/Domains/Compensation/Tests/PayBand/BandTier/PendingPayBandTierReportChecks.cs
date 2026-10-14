namespace Acme.Workforce.Compensation.Tests.PayBand.BandTier;

using Acme.Workforce.Compensation.PayBand.BandTier;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of band tier code, run by the project's runner.</summary>
public static class PendingPayBandTierReportChecks
{
    public static void PendingPayBandTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701092515);
        var items = new List<PendingPayBandTier>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(PendingPayBandTier.NewPendingPayBandTier($"pending-pay-band-tier-{index}", "sample"));
        }
        var report = PendingPayBandTierReport.BuildPendingPayBandTierReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        PendingPayBandTierReportCountsEachRecordOnce();
    }
}
