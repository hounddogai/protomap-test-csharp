namespace Acme.Workforce.Payrollops.Tests.TaxForm.Tier;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.TaxForm.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class TaxFormExceptionReportChecks
{
    public static void TaxFormExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704381887);
        var items = new List<TaxFormException>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(TaxFormException.NewTaxFormException($"tax-form-exception-{index}", "sample"));
        }
        var report = TaxFormExceptionReport.BuildTaxFormExceptionReport("test", items, clock);
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
        TaxFormExceptionReportCountsEachRecordOnce();
    }
}
