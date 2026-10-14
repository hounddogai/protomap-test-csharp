namespace Acme.Workforce.Orgdesign.Tests.CostModel.Summary;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.CostModel.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class CostModelSummaryReportChecks
{
    public static void CostModelSummaryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708553527);
        var items = new List<CostModelSummary>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(CostModelSummary.NewCostModelSummary($"cost-model-summary-{index}", "sample"));
        }
        var report = CostModelSummaryReport.BuildCostModelSummaryReport("test", items, clock);
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
        CostModelSummaryReportCountsEachRecordOnce();
    }
}
