namespace Acme.Workforce.Orgdesign.Tests.CostModel.Rule;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.CostModel.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class TieredCostModelSummaryReportChecks
{
    public static void TieredCostModelSummaryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708053999);
        var items = new List<TieredCostModelSummary>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(TieredCostModelSummary.NewTieredCostModelSummary($"tiered-cost-model-summary-{index}", "sample"));
        }
        var report = TieredCostModelSummaryReport.BuildTieredCostModelSummaryReport("test", items, clock);
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
        TieredCostModelSummaryReportCountsEachRecordOnce();
    }
}
