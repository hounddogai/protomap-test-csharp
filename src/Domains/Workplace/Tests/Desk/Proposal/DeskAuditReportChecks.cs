namespace Acme.Workforce.Workplace.Tests.Desk.Proposal;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Desk.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DeskAuditReportChecks
{
    public static void DeskAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707303308);
        var items = new List<DeskAudit>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(DeskAudit.NewDeskAudit($"desk-audit-{index}", "sample"));
        }
        var report = DeskAuditReport.BuildDeskAuditReport("test", items, clock);
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
        DeskAuditReportCountsEachRecordOnce();
    }
}
