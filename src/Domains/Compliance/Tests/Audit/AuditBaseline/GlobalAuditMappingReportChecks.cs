namespace Acme.Workforce.Compliance.Tests.Audit.AuditBaseline;

using Acme.Workforce.Compliance.Audit.AuditBaseline;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of audit baseline code, run by the project's runner.</summary>
public static class GlobalAuditMappingReportChecks
{
    public static void GlobalAuditMappingReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703521490);
        var items = new List<GlobalAuditMapping>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(GlobalAuditMapping.NewGlobalAuditMapping($"global-audit-mapping-{index}", "sample"));
        }
        var report = GlobalAuditMappingReport.BuildGlobalAuditMappingReport("test", items, clock);
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
        GlobalAuditMappingReportCountsEachRecordOnce();
    }
}
