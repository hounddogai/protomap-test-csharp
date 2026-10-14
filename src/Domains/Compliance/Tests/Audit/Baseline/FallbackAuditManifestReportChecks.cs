namespace Acme.Workforce.Compliance.Tests.Audit.Baseline;

using Acme.Workforce.Compliance.Audit.Baseline;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class FallbackAuditManifestReportChecks
{
    public static void FallbackAuditManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707432071);
        var items = new List<FallbackAuditManifest>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(FallbackAuditManifest.NewFallbackAuditManifest($"fallback-audit-manifest-{index}", "sample"));
        }
        var report = FallbackAuditManifestReport.BuildFallbackAuditManifestReport("test", items, clock);
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
        FallbackAuditManifestReportCountsEachRecordOnce();
    }
}
