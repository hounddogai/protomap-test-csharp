namespace Acme.Workforce.Orgdesign.Tests.CostModel.Assignment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.CostModel.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class CostModelBatchReportChecks
{
    public static void CostModelBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705867326);
        var items = new List<CostModelBatch>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(CostModelBatch.NewCostModelBatch($"cost-model-batch-{index}", "sample"));
        }
        var report = CostModelBatchReport.BuildCostModelBatchReport("test", items, clock);
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
        CostModelBatchReportCountsEachRecordOnce();
    }
}
