namespace Acme.Workforce.Payrollops.Tests.Garnishment.GarnishmentAgreement;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Garnishment.GarnishmentAgreement;

/// <summary>Checks of garnishment agreement code, run by the project's runner.</summary>
public static class LegacyGarnishmentMetricReportChecks
{
    public static void LegacyGarnishmentMetricReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705452274);
        var items = new List<LegacyGarnishmentMetric>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LegacyGarnishmentMetric.NewLegacyGarnishmentMetric($"legacy-garnishment-metric-{index}", "sample"));
        }
        var report = LegacyGarnishmentMetricReport.BuildLegacyGarnishmentMetricReport("test", items, clock);
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
        LegacyGarnishmentMetricReportCountsEachRecordOnce();
    }
}
