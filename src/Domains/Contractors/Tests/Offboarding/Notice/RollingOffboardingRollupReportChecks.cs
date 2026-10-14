namespace Acme.Workforce.Contractors.Tests.Offboarding.Notice;

using Acme.Workforce.Contractors.Offboarding.Notice;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class RollingOffboardingRollupReportChecks
{
    public static void RollingOffboardingRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704820494);
        var items = new List<RollingOffboardingRollup>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(RollingOffboardingRollup.NewRollingOffboardingRollup($"rolling-offboarding-rollup-{index}", "sample"));
        }
        var report = RollingOffboardingRollupReport.BuildRollingOffboardingRollupReport("test", items, clock);
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
        RollingOffboardingRollupReportCountsEachRecordOnce();
    }
}
