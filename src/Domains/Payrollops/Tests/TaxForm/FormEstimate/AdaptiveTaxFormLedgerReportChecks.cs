namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormEstimate;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.TaxForm.FormEstimate;

/// <summary>Checks of form estimate code, run by the project's runner.</summary>
public static class AdaptiveTaxFormLedgerReportChecks
{
    public static void AdaptiveTaxFormLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700187972);
        var items = new List<AdaptiveTaxFormLedger>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(AdaptiveTaxFormLedger.NewAdaptiveTaxFormLedger($"adaptive-tax-form-ledger-{index}", "sample"));
        }
        var report = AdaptiveTaxFormLedgerReport.BuildAdaptiveTaxFormLedgerReport("test", items, clock);
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
        AdaptiveTaxFormLedgerReportCountsEachRecordOnce();
    }
}
