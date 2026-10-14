namespace Acme.Workforce.Compliance.Tests.Attestation.Window;

using Acme.Workforce.Compliance.Attestation.Window;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class GlobalAttestationBundleReportChecks
{
    public static void GlobalAttestationBundleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704161254);
        var items = new List<GlobalAttestationBundle>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(GlobalAttestationBundle.NewGlobalAttestationBundle($"global-attestation-bundle-{index}", "sample"));
        }
        var report = GlobalAttestationBundleReport.BuildGlobalAttestationBundleReport("test", items, clock);
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
        GlobalAttestationBundleReportCountsEachRecordOnce();
    }
}
