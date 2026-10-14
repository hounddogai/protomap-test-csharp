namespace Acme.Workforce.Contractors.Tests.Offboarding.OffboardingBundle;

using Acme.Workforce.Contractors.Offboarding.OffboardingBundle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of offboarding bundle code, run by the project's runner.</summary>
public static class StrictOffboardingBundleReportChecks
{
    public static void StrictOffboardingBundleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706892633);
        var items = new List<StrictOffboardingBundle>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(StrictOffboardingBundle.NewStrictOffboardingBundle($"strict-offboarding-bundle-{index}", "sample"));
        }
        var report = StrictOffboardingBundleReport.BuildStrictOffboardingBundleReport("test", items, clock);
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
        StrictOffboardingBundleReportCountsEachRecordOnce();
    }
}
