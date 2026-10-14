namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormGroup;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.TaxForm.FormGroup;

/// <summary>Checks of form group code, run by the project's runner.</summary>
public static class ManualTaxFormMetricReportChecks
{
    public static void ManualTaxFormMetricReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702580626);
        var items = new List<ManualTaxFormMetric>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(ManualTaxFormMetric.NewManualTaxFormMetric($"manual-tax-form-metric-{index}", "sample"));
        }
        var report = ManualTaxFormMetricReport.BuildManualTaxFormMetricReport("test", items, clock);
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
        ManualTaxFormMetricReportCountsEachRecordOnce();
    }
}
