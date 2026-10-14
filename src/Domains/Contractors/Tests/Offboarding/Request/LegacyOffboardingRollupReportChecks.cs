namespace Acme.Workforce.Contractors.Tests.Offboarding.Request;

using Acme.Workforce.Contractors.Offboarding.Request;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class LegacyOffboardingRollupReportChecks
{
    public static void LegacyOffboardingRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709546670);
        var items = new List<LegacyOffboardingRollup>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LegacyOffboardingRollup.NewLegacyOffboardingRollup($"legacy-offboarding-rollup-{index}", "sample"));
        }
        var report = LegacyOffboardingRollupReport.BuildLegacyOffboardingRollupReport("test", items, clock);
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
        LegacyOffboardingRollupReportCountsEachRecordOnce();
    }
}
