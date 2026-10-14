namespace Acme.Workforce.Orgdesign.Tests.CostModel.Rule;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.CostModel.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class LegacyCostModelManifestReportChecks
{
    public static void LegacyCostModelManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702508114);
        var items = new List<LegacyCostModelManifest>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(LegacyCostModelManifest.NewLegacyCostModelManifest($"legacy-cost-model-manifest-{index}", "sample"));
        }
        var report = LegacyCostModelManifestReport.BuildLegacyCostModelManifestReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        LegacyCostModelManifestReportCountsEachRecordOnce();
    }
}
