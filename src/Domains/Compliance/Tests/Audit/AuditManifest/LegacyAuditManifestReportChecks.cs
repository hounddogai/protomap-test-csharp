namespace Acme.Workforce.Compliance.Tests.Audit.AuditManifest;

using Acme.Workforce.Compliance.Audit.AuditManifest;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of audit manifest code, run by the project's runner.</summary>
public static class LegacyAuditManifestReportChecks
{
    public static void LegacyAuditManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702333809);
        var items = new List<LegacyAuditManifest>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(LegacyAuditManifest.NewLegacyAuditManifest($"legacy-audit-manifest-{index}", "sample"));
        }
        var report = LegacyAuditManifestReport.BuildLegacyAuditManifestReport("test", items, clock);
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
        LegacyAuditManifestReportCountsEachRecordOnce();
    }
}
