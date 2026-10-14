namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Target;

using Acme.Workforce.Contractors.StatementOfWork.Target;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class AdaptiveStatementOfWorkAuditReportChecks
{
    public static void AdaptiveStatementOfWorkAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705106783);
        var items = new List<AdaptiveStatementOfWorkAudit>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(AdaptiveStatementOfWorkAudit.NewAdaptiveStatementOfWorkAudit($"adaptive-statement-of-work-audit-{index}", "sample"));
        }
        var report = AdaptiveStatementOfWorkAuditReport.BuildAdaptiveStatementOfWorkAuditReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        AdaptiveStatementOfWorkAuditReportCountsEachRecordOnce();
    }
}
