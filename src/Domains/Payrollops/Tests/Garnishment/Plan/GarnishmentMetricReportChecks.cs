namespace Acme.Workforce.Payrollops.Tests.Garnishment.Plan;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Garnishment.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class GarnishmentMetricReportChecks
{
    public static void GarnishmentMetricReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701590053);
        var items = new List<GarnishmentMetric>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(GarnishmentMetric.NewGarnishmentMetric($"garnishment-metric-{index}", "sample"));
        }
        var report = GarnishmentMetricReport.BuildGarnishmentMetricReport("test", items, clock);
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
        GarnishmentMetricReportCountsEachRecordOnce();
    }
}
