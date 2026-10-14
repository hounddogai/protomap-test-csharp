namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Schedule;

using Acme.Workforce.Contractors.StatementOfWork.Schedule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class WeightedStatementOfWorkBundleReportChecks
{
    public static void WeightedStatementOfWorkBundleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708058061);
        var items = new List<WeightedStatementOfWorkBundle>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(WeightedStatementOfWorkBundle.NewWeightedStatementOfWorkBundle($"weighted-statement-of-work-bundle-{index}", "sample"));
        }
        var report = WeightedStatementOfWorkBundleReport.BuildWeightedStatementOfWorkBundleReport("test", items, clock);
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
        WeightedStatementOfWorkBundleReportCountsEachRecordOnce();
    }
}
