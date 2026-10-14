namespace Acme.Workforce.Payrollops.Tests.TaxForm.Exception;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.TaxForm.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class LegacyTaxFormExceptionReportChecks
{
    public static void LegacyTaxFormExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704524999);
        var items = new List<LegacyTaxFormException>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LegacyTaxFormException.NewLegacyTaxFormException($"legacy-tax-form-exception-{index}", "sample"));
        }
        var report = LegacyTaxFormExceptionReport.BuildLegacyTaxFormExceptionReport("test", items, clock);
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
        LegacyTaxFormExceptionReportCountsEachRecordOnce();
    }
}
