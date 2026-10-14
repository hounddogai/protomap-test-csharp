namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Window;

using Acme.Workforce.Contractors.StatementOfWork.Window;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class StatementOfWorkWindowReportChecks
{
    public static void StatementOfWorkWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709862466);
        var items = new List<StatementOfWorkWindow>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(StatementOfWorkWindow.NewStatementOfWorkWindow($"statement-of-work-window-{index}", "sample"));
        }
        var report = StatementOfWorkWindowReport.BuildStatementOfWorkWindowReport("test", items, clock);
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
        StatementOfWorkWindowReportCountsEachRecordOnce();
    }
}
