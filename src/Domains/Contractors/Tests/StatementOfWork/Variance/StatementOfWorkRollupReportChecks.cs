namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Variance;

using Acme.Workforce.Contractors.StatementOfWork.Variance;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class StatementOfWorkRollupReportChecks
{
    public static void StatementOfWorkRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703128745);
        var items = new List<StatementOfWorkRollup>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(StatementOfWorkRollup.NewStatementOfWorkRollup($"statement-of-work-rollup-{index}", "sample"));
        }
        var report = StatementOfWorkRollupReport.BuildStatementOfWorkRollupReport("test", items, clock);
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
        StatementOfWorkRollupReportCountsEachRecordOnce();
    }
}
