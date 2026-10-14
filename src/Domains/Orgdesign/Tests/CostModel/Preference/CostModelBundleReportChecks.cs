namespace Acme.Workforce.Orgdesign.Tests.CostModel.Preference;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.CostModel.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class CostModelBundleReportChecks
{
    public static void CostModelBundleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702761668);
        var items = new List<CostModelBundle>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(CostModelBundle.NewCostModelBundle($"cost-model-bundle-{index}", "sample"));
        }
        var report = CostModelBundleReport.BuildCostModelBundleReport("test", items, clock);
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
        CostModelBundleReportCountsEachRecordOnce();
    }
}
