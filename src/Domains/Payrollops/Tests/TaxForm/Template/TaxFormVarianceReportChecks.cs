namespace Acme.Workforce.Payrollops.Tests.TaxForm.Template;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.TaxForm.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class TaxFormVarianceReportChecks
{
    public static void TaxFormVarianceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706679326);
        var items = new List<TaxFormVariance>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(TaxFormVariance.NewTaxFormVariance($"tax-form-variance-{index}", "sample"));
        }
        var report = TaxFormVarianceReport.BuildTaxFormVarianceReport("test", items, clock);
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
        TaxFormVarianceReportCountsEachRecordOnce();
    }
}
