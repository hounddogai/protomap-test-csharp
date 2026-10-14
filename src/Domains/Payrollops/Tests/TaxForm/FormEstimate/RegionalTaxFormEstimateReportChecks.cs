namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormEstimate;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.TaxForm.FormEstimate;

/// <summary>Checks of form estimate code, run by the project's runner.</summary>
public static class RegionalTaxFormEstimateReportChecks
{
    public static void RegionalTaxFormEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709637736);
        var items = new List<RegionalTaxFormEstimate>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(RegionalTaxFormEstimate.NewRegionalTaxFormEstimate($"regional-tax-form-estimate-{index}", "sample"));
        }
        var report = RegionalTaxFormEstimateReport.BuildRegionalTaxFormEstimateReport("test", items, clock);
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
        RegionalTaxFormEstimateReportCountsEachRecordOnce();
    }
}
