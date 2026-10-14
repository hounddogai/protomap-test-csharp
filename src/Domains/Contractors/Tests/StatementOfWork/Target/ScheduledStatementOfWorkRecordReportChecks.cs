namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Target;

using Acme.Workforce.Contractors.StatementOfWork.Target;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class ScheduledStatementOfWorkRecordReportChecks
{
    public static void ScheduledStatementOfWorkRecordReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707259516);
        var items = new List<ScheduledStatementOfWorkRecord>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(ScheduledStatementOfWorkRecord.NewScheduledStatementOfWorkRecord($"scheduled-statement-of-work-record-{index}", "sample"));
        }
        var report = ScheduledStatementOfWorkRecordReport.BuildScheduledStatementOfWorkRecordReport("test", items, clock);
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
        ScheduledStatementOfWorkRecordReportCountsEachRecordOnce();
    }
}
