namespace Acme.Workforce.Payrollops.Tests.Deduction.Rollup;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Deduction.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class DeductionVarianceReportChecks
{
    public static void DeductionVarianceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704330041);
        var items = new List<DeductionVariance>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(DeductionVariance.NewDeductionVariance($"deduction-variance-{index}", "sample"));
        }
        var report = DeductionVarianceReport.BuildDeductionVarianceReport("test", items, clock);
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
        DeductionVarianceReportCountsEachRecordOnce();
    }
}
