namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelMetric;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.CostModel.ModelMetric;

/// <summary>Checks of model metric code, run by the project's runner.</summary>
public static class RegionalCostModelSummaryReportChecks
{
    public static void RegionalCostModelSummaryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705133685);
        var items = new List<RegionalCostModelSummary>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(RegionalCostModelSummary.NewRegionalCostModelSummary($"regional-cost-model-summary-{index}", "sample"));
        }
        var report = RegionalCostModelSummaryReport.BuildRegionalCostModelSummaryReport("test", items, clock);
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
        RegionalCostModelSummaryReportCountsEachRecordOnce();
    }
}
