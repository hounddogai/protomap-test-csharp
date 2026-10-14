namespace Acme.Workforce.Orgdesign.Tests.CostModel.Summary;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.CostModel.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class StrictCostModelCheckpointReportChecks
{
    public static void StrictCostModelCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701580153);
        var items = new List<StrictCostModelCheckpoint>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(StrictCostModelCheckpoint.NewStrictCostModelCheckpoint($"strict-cost-model-checkpoint-{index}", "sample"));
        }
        var report = StrictCostModelCheckpointReport.BuildStrictCostModelCheckpointReport("test", items, clock);
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
        StrictCostModelCheckpointReportCountsEachRecordOnce();
    }
}
