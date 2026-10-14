namespace Acme.Workforce.Assets.Tests.AssetReturn.OverrideItem;

using Acme.Workforce.Assets.AssetReturn.OverrideItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class AssetReturnTierReportChecks
{
    public static void AssetReturnTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707492087);
        var items = new List<AssetReturnTier>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(AssetReturnTier.NewAssetReturnTier($"asset-return-tier-{index}", "sample"));
        }
        var report = AssetReturnTierReport.BuildAssetReturnTierReport("test", items, clock);
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
        AssetReturnTierReportCountsEachRecordOnce();
    }
}
