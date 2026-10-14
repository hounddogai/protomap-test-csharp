namespace Acme.Workforce.Payrollops.Tests.TaxForm.Ledger;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.TaxForm.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class TaxFormRequestReportChecks
{
    public static void TaxFormRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703577603);
        var items = new List<TaxFormRequest>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(TaxFormRequest.NewTaxFormRequest($"tax-form-request-{index}", "sample"));
        }
        var report = TaxFormRequestReport.BuildTaxFormRequestReport("test", items, clock);
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
        TaxFormRequestReportCountsEachRecordOnce();
    }
}
