namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelGroup;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.CostModel.ModelGroup;

/// <summary>Checks of model group code, run by the project's runner.</summary>
public static class RollingCostModelBatchReportChecks
{
    public static void RollingCostModelBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705913538);
        var items = new List<RollingCostModelBatch>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(RollingCostModelBatch.NewRollingCostModelBatch($"rolling-cost-model-batch-{index}", "sample"));
        }
        var report = RollingCostModelBatchReport.BuildRollingCostModelBatchReport("test", items, clock);
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
        RollingCostModelBatchReportCountsEachRecordOnce();
    }
}
