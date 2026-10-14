namespace Acme.Workforce.Payrollops.Tests.TaxForm.Adjustment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.TaxForm.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class TaxFormRecordReportChecks
{
    public static void TaxFormRecordReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704385325);
        var items = new List<TaxFormRecord>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(TaxFormRecord.NewTaxFormRecord($"tax-form-record-{index}", "sample"));
        }
        var report = TaxFormRecordReport.BuildTaxFormRecordReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        TaxFormRecordReportCountsEachRecordOnce();
    }
}
