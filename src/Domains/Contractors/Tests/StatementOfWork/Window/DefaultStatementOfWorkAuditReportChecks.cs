namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Window;

using Acme.Workforce.Contractors.StatementOfWork.Window;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class DefaultStatementOfWorkAuditReportChecks
{
    public static void DefaultStatementOfWorkAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704029525);
        var items = new List<DefaultStatementOfWorkAudit>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(DefaultStatementOfWorkAudit.NewDefaultStatementOfWorkAudit($"default-statement-of-work-audit-{index}", "sample"));
        }
        var report = DefaultStatementOfWorkAuditReport.BuildDefaultStatementOfWorkAuditReport("test", items, clock);
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
        DefaultStatementOfWorkAuditReportCountsEachRecordOnce();
    }
}
