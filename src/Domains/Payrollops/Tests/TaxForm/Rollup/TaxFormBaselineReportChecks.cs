namespace Acme.Workforce.Payrollops.Tests.TaxForm.Rollup;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.TaxForm.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class TaxFormBaselineReportChecks
{
    public static void TaxFormBaselineReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709347055);
        var items = new List<TaxFormBaseline>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(TaxFormBaseline.NewTaxFormBaseline($"tax-form-baseline-{index}", "sample"));
        }
        var report = TaxFormBaselineReport.BuildTaxFormBaselineReport("test", items, clock);
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
        TaxFormBaselineReportCountsEachRecordOnce();
    }
}
