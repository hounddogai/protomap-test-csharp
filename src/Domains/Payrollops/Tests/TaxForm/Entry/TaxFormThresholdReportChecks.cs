namespace Acme.Workforce.Payrollops.Tests.TaxForm.Entry;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.TaxForm.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class TaxFormThresholdReportChecks
{
    public static void TaxFormThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705526636);
        var items = new List<TaxFormThreshold>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(TaxFormThreshold.NewTaxFormThreshold($"tax-form-threshold-{index}", "sample"));
        }
        var report = TaxFormThresholdReport.BuildTaxFormThresholdReport("test", items, clock);
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
        TaxFormThresholdReportCountsEachRecordOnce();
    }
}
