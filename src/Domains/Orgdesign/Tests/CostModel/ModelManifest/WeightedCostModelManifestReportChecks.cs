namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelManifest;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.CostModel.ModelManifest;

/// <summary>Checks of model manifest code, run by the project's runner.</summary>
public static class WeightedCostModelManifestReportChecks
{
    public static void WeightedCostModelManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703504216);
        var items = new List<WeightedCostModelManifest>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(WeightedCostModelManifest.NewWeightedCostModelManifest($"weighted-cost-model-manifest-{index}", "sample"));
        }
        var report = WeightedCostModelManifestReport.BuildWeightedCostModelManifestReport("test", items, clock);
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
        WeightedCostModelManifestReportCountsEachRecordOnce();
    }
}
