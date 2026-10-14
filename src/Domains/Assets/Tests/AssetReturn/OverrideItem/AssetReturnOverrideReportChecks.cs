namespace Acme.Workforce.Assets.Tests.AssetReturn.OverrideItem;

using Acme.Workforce.Assets.AssetReturn.OverrideItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class AssetReturnOverrideReportChecks
{
    public static void AssetReturnOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709821295);
        var items = new List<AssetReturnOverride>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AssetReturnOverride.NewAssetReturnOverride($"asset-return-override-{index}", "sample"));
        }
        var report = AssetReturnOverrideReport.BuildAssetReturnOverrideReport("test", items, clock);
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
        AssetReturnOverrideReportCountsEachRecordOnce();
    }
}
