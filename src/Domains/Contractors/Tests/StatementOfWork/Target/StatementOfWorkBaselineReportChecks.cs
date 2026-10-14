namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Target;

using Acme.Workforce.Contractors.StatementOfWork.Target;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class StatementOfWorkBaselineReportChecks
{
    public static void StatementOfWorkBaselineReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702724929);
        var items = new List<StatementOfWorkBaseline>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(StatementOfWorkBaseline.NewStatementOfWorkBaseline($"statement-of-work-baseline-{index}", "sample"));
        }
        var report = StatementOfWorkBaselineReport.BuildStatementOfWorkBaselineReport("test", items, clock);
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
        StatementOfWorkBaselineReportCountsEachRecordOnce();
    }
}
