namespace Acme.Workforce.Payrollops.Tests.Deduction.Exception;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Deduction.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class WeightedDeductionPlanReportChecks
{
    public static void WeightedDeductionPlanReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704826884);
        var items = new List<WeightedDeductionPlan>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(WeightedDeductionPlan.NewWeightedDeductionPlan($"weighted-deduction-plan-{index}", "sample"));
        }
        var report = WeightedDeductionPlanReport.BuildWeightedDeductionPlanReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        WeightedDeductionPlanReportCountsEachRecordOnce();
    }
}
