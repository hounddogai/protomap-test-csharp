namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Schedule;

using Acme.Workforce.Contractors.StatementOfWork.Schedule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class StrictStatementOfWorkAssignmentReportChecks
{
    public static void StrictStatementOfWorkAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702773541);
        var items = new List<StrictStatementOfWorkAssignment>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(StrictStatementOfWorkAssignment.NewStrictStatementOfWorkAssignment($"strict-statement-of-work-assignment-{index}", "sample"));
        }
        var report = StrictStatementOfWorkAssignmentReport.BuildStrictStatementOfWorkAssignmentReport("test", items, clock);
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
        StrictStatementOfWorkAssignmentReportCountsEachRecordOnce();
    }
}
