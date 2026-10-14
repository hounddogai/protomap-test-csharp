namespace Acme.Workforce.Contractors.Tests.Offboarding.Threshold;

using Acme.Workforce.Contractors.Offboarding.Threshold;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class OffboardingManifestReportChecks
{
    public static void OffboardingManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701464215);
        var items = new List<OffboardingManifest>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(OffboardingManifest.NewOffboardingManifest($"offboarding-manifest-{index}", "sample"));
        }
        var report = OffboardingManifestReport.BuildOffboardingManifestReport("test", items, clock);
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
        OffboardingManifestReportCountsEachRecordOnce();
    }
}
