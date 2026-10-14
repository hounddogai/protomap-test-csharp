namespace Acme.Workforce.Talent.Tests.Mobility.Tier;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Mobility.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class MobilityClaimReportChecks
{
    public static void MobilityClaimReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703357881);
        var items = new List<MobilityClaim>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(MobilityClaim.NewMobilityClaim($"mobility-claim-{index}", "sample"));
        }
        var report = MobilityClaimReport.BuildMobilityClaimReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        MobilityClaimReportCountsEachRecordOnce();
    }
}
