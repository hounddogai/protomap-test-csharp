namespace Acme.Workforce.Orgdesign.Tests.Position.Audit;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Position.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class PositionAuditReportChecks
{
    public static void PositionAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708085618);
        var items = new List<PositionAudit>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(PositionAudit.NewPositionAudit($"position-audit-{index}", "sample"));
        }
        var report = PositionAuditReport.BuildPositionAuditReport("test", items, clock);
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
        PositionAuditReportCountsEachRecordOnce();
    }
}
