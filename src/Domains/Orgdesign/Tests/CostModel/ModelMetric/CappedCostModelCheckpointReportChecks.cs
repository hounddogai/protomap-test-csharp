namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelMetric;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.CostModel.ModelMetric;

/// <summary>Checks of model metric code, run by the project's runner.</summary>
public static class CappedCostModelCheckpointReportChecks
{
    public static void CappedCostModelCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704060068);
        var items = new List<CappedCostModelCheckpoint>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(CappedCostModelCheckpoint.NewCappedCostModelCheckpoint($"capped-cost-model-checkpoint-{index}", "sample"));
        }
        var report = CappedCostModelCheckpointReport.BuildCappedCostModelCheckpointReport("test", items, clock);
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
        CappedCostModelCheckpointReportCountsEachRecordOnce();
    }
}
