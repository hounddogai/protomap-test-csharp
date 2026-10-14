namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelPreference;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.CostModel.ModelPreference;

/// <summary>Checks of model preference code, run by the project's runner.</summary>
public static class CostModelThresholdReportChecks
{
    public static void CostModelThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705028283);
        var items = new List<CostModelThreshold>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(CostModelThreshold.NewCostModelThreshold($"cost-model-threshold-{index}", "sample"));
        }
        var report = CostModelThresholdReport.BuildCostModelThresholdReport("test", items, clock);
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
        CostModelThresholdReportCountsEachRecordOnce();
    }
}
