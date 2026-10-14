namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentRequest;

using Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of adjustment request code, run by the project's runner.</summary>
public static class WeightedAdjustmentTemplateReportChecks
{
    public static void WeightedAdjustmentTemplateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708416365);
        var items = new List<WeightedAdjustmentTemplate>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(WeightedAdjustmentTemplate.NewWeightedAdjustmentTemplate($"weighted-adjustment-template-{index}", "sample"));
        }
        var report = WeightedAdjustmentTemplateReport.BuildWeightedAdjustmentTemplateReport("test", items, clock);
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
        WeightedAdjustmentTemplateReportCountsEachRecordOnce();
    }
}
