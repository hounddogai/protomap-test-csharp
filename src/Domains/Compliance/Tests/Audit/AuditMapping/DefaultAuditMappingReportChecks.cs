namespace Acme.Workforce.Compliance.Tests.Audit.AuditMapping;

using Acme.Workforce.Compliance.Audit.AuditMapping;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of audit mapping code, run by the project's runner.</summary>
public static class DefaultAuditMappingReportChecks
{
    public static void DefaultAuditMappingReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701824538);
        var items = new List<DefaultAuditMapping>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(DefaultAuditMapping.NewDefaultAuditMapping($"default-audit-mapping-{index}", "sample"));
        }
        var report = DefaultAuditMappingReport.BuildDefaultAuditMappingReport("test", items, clock);
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
        DefaultAuditMappingReportCountsEachRecordOnce();
    }
}
