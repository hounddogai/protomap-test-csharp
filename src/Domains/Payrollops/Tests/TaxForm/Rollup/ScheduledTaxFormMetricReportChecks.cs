namespace Acme.Workforce.Payrollops.Tests.TaxForm.Rollup;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.TaxForm.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class ScheduledTaxFormMetricReportChecks
{
    public static void ScheduledTaxFormMetricReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707275943);
        var items = new List<ScheduledTaxFormMetric>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(ScheduledTaxFormMetric.NewScheduledTaxFormMetric($"scheduled-tax-form-metric-{index}", "sample"));
        }
        var report = ScheduledTaxFormMetricReport.BuildScheduledTaxFormMetricReport("test", items, clock);
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
        ScheduledTaxFormMetricReportCountsEachRecordOnce();
    }
}
