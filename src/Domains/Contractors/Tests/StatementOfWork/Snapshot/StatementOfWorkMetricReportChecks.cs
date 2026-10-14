namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Snapshot;

using Acme.Workforce.Contractors.StatementOfWork.Snapshot;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class StatementOfWorkMetricReportChecks
{
    public static void StatementOfWorkMetricReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705833241);
        var items = new List<StatementOfWorkMetric>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(StatementOfWorkMetric.NewStatementOfWorkMetric($"statement-of-work-metric-{index}", "sample"));
        }
        var report = StatementOfWorkMetricReport.BuildStatementOfWorkMetricReport("test", items, clock);
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
        StatementOfWorkMetricReportCountsEachRecordOnce();
    }
}
