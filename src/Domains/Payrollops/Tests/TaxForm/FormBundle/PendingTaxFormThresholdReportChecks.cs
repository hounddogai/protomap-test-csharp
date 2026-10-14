namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormBundle;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.TaxForm.FormBundle;

/// <summary>Checks of form bundle code, run by the project's runner.</summary>
public static class PendingTaxFormThresholdReportChecks
{
    public static void PendingTaxFormThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708817375);
        var items = new List<PendingTaxFormThreshold>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PendingTaxFormThreshold.NewPendingTaxFormThreshold($"pending-tax-form-threshold-{index}", "sample"));
        }
        var report = PendingTaxFormThresholdReport.BuildPendingTaxFormThresholdReport("test", items, clock);
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
        PendingTaxFormThresholdReportCountsEachRecordOnce();
    }
}
