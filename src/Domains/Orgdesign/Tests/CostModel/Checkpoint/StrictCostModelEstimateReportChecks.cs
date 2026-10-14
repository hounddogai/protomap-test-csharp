namespace Acme.Workforce.Orgdesign.Tests.CostModel.Checkpoint;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.CostModel.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class StrictCostModelEstimateReportChecks
{
    public static void StrictCostModelEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701411414);
        var items = new List<StrictCostModelEstimate>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(StrictCostModelEstimate.NewStrictCostModelEstimate($"strict-cost-model-estimate-{index}", "sample"));
        }
        var report = StrictCostModelEstimateReport.BuildStrictCostModelEstimateReport("test", items, clock);
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
        StrictCostModelEstimateReportCountsEachRecordOnce();
    }
}
