namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Summary;

using Acme.Workforce.Contractors.StatementOfWork.Summary;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class StatementOfWorkRequestReportChecks
{
    public static void StatementOfWorkRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707707997);
        var items = new List<StatementOfWorkRequest>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(StatementOfWorkRequest.NewStatementOfWorkRequest($"statement-of-work-request-{index}", "sample"));
        }
        var report = StatementOfWorkRequestReport.BuildStatementOfWorkRequestReport("test", items, clock);
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
        StatementOfWorkRequestReportCountsEachRecordOnce();
    }
}
