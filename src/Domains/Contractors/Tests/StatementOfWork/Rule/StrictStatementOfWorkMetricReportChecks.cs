namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Rule;

using Acme.Workforce.Contractors.StatementOfWork.Rule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class StrictStatementOfWorkMetricReportChecks
{
    public static void StrictStatementOfWorkMetricReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708170612);
        var items = new List<StrictStatementOfWorkMetric>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(StrictStatementOfWorkMetric.NewStrictStatementOfWorkMetric($"strict-statement-of-work-metric-{index}", "sample"));
        }
        var report = StrictStatementOfWorkMetricReport.BuildStrictStatementOfWorkMetricReport("test", items, clock);
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
        StrictStatementOfWorkMetricReportCountsEachRecordOnce();
    }
}
