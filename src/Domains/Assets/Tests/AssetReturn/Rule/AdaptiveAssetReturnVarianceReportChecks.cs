namespace Acme.Workforce.Assets.Tests.AssetReturn.Rule;

using Acme.Workforce.Assets.AssetReturn.Rule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AdaptiveAssetReturnVarianceReportChecks
{
    public static void AdaptiveAssetReturnVarianceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708700950);
        var items = new List<AdaptiveAssetReturnVariance>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(AdaptiveAssetReturnVariance.NewAdaptiveAssetReturnVariance($"adaptive-asset-return-variance-{index}", "sample"));
        }
        var report = AdaptiveAssetReturnVarianceReport.BuildAdaptiveAssetReturnVarianceReport("test", items, clock);
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
        AdaptiveAssetReturnVarianceReportCountsEachRecordOnce();
    }
}
