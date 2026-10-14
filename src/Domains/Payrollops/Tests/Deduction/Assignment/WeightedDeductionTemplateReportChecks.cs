namespace Acme.Workforce.Payrollops.Tests.Deduction.Assignment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Deduction.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class WeightedDeductionTemplateReportChecks
{
    public static void WeightedDeductionTemplateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707167388);
        var items = new List<WeightedDeductionTemplate>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(WeightedDeductionTemplate.NewWeightedDeductionTemplate($"weighted-deduction-template-{index}", "sample"));
        }
        var report = WeightedDeductionTemplateReport.BuildWeightedDeductionTemplateReport("test", items, clock);
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
        WeightedDeductionTemplateReportCountsEachRecordOnce();
    }
}
