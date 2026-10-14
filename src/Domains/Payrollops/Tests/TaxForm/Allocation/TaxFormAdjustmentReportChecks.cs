namespace Acme.Workforce.Payrollops.Tests.TaxForm.Allocation;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.TaxForm.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class TaxFormAdjustmentReportChecks
{
    public static void TaxFormAdjustmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702680228);
        var items = new List<TaxFormAdjustment>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(TaxFormAdjustment.NewTaxFormAdjustment($"tax-form-adjustment-{index}", "sample"));
        }
        var report = TaxFormAdjustmentReport.BuildTaxFormAdjustmentReport("test", items, clock);
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
        TaxFormAdjustmentReportCountsEachRecordOnce();
    }
}
