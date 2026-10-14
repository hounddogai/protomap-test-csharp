namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Summary;

using Acme.Workforce.Contractors.StatementOfWork.Summary;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class StatementOfWorkQuotaReportChecks
{
    public static void StatementOfWorkQuotaReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705468803);
        var items = new List<StatementOfWorkQuota>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(StatementOfWorkQuota.NewStatementOfWorkQuota($"statement-of-work-quota-{index}", "sample"));
        }
        var report = StatementOfWorkQuotaReport.BuildStatementOfWorkQuotaReport("test", items, clock);
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
        StatementOfWorkQuotaReportCountsEachRecordOnce();
    }
}
