namespace Acme.Workforce.Payrollops.Tests.Deduction.Agreement;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Deduction.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class DeductionAdjustmentReportChecks
{
    public static void DeductionAdjustmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708199859);
        var items = new List<DeductionAdjustment>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(DeductionAdjustment.NewDeductionAdjustment($"deduction-adjustment-{index}", "sample"));
        }
        var report = DeductionAdjustmentReport.BuildDeductionAdjustmentReport("test", items, clock);
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
        DeductionAdjustmentReportCountsEachRecordOnce();
    }
}
