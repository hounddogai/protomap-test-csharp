namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Profile;

using Acme.Workforce.Contractors.StatementOfWork.Profile;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class CappedStatementOfWorkExceptionReportChecks
{
    public static void CappedStatementOfWorkExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702055462);
        var items = new List<CappedStatementOfWorkException>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(CappedStatementOfWorkException.NewCappedStatementOfWorkException($"capped-statement-of-work-exception-{index}", "sample"));
        }
        var report = CappedStatementOfWorkExceptionReport.BuildCappedStatementOfWorkExceptionReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        CappedStatementOfWorkExceptionReportCountsEachRecordOnce();
    }
}
