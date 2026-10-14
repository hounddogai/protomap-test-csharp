namespace Acme.Workforce.Compensation.Tests.Adjustment.BatchedAdjustmentRule;

using Acme.Workforce.Compensation.Adjustment.BatchedAdjustmentRule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of batched adjustment rule code, run by the project's runner.</summary>
public static class ManualAdjustmentRuleReportChecks
{
    public static void ManualAdjustmentRuleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708808600);
        var items = new List<ManualAdjustmentRule>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(ManualAdjustmentRule.NewManualAdjustmentRule($"manual-adjustment-rule-{index}", "sample"));
        }
        var report = ManualAdjustmentRuleReport.BuildManualAdjustmentRuleReport("test", items, clock);
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
        ManualAdjustmentRuleReportCountsEachRecordOnce();
    }
}
