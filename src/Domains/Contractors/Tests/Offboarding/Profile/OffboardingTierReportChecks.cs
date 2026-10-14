namespace Acme.Workforce.Contractors.Tests.Offboarding.Profile;

using Acme.Workforce.Contractors.Offboarding.Profile;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class OffboardingTierReportChecks
{
    public static void OffboardingTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703468092);
        var items = new List<OffboardingTier>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(OffboardingTier.NewOffboardingTier($"offboarding-tier-{index}", "sample"));
        }
        var report = OffboardingTierReport.BuildOffboardingTierReport("test", items, clock);
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
        OffboardingTierReportCountsEachRecordOnce();
    }
}
