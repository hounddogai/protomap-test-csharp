namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Snapshot;

using Acme.Workforce.Contractors.StatementOfWork.Snapshot;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class StatementOfWorkExceptionReportChecks
{
    public static void StatementOfWorkExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703753546);
        var items = new List<StatementOfWorkException>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(StatementOfWorkException.NewStatementOfWorkException($"statement-of-work-exception-{index}", "sample"));
        }
        var report = StatementOfWorkExceptionReport.BuildStatementOfWorkExceptionReport("test", items, clock);
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
        StatementOfWorkExceptionReportCountsEachRecordOnce();
    }
}
