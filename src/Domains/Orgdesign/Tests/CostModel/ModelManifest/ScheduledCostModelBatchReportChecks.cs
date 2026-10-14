namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelManifest;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.CostModel.ModelManifest;

/// <summary>Checks of model manifest code, run by the project's runner.</summary>
public static class ScheduledCostModelBatchReportChecks
{
    public static void ScheduledCostModelBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704195151);
        var items = new List<ScheduledCostModelBatch>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(ScheduledCostModelBatch.NewScheduledCostModelBatch($"scheduled-cost-model-batch-{index}", "sample"));
        }
        var report = ScheduledCostModelBatchReport.BuildScheduledCostModelBatchReport("test", items, clock);
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
        ScheduledCostModelBatchReportCountsEachRecordOnce();
    }
}
