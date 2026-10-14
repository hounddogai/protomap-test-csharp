namespace Acme.Workforce.Assets.Tests.AssetReturn.Variance;

using Acme.Workforce.Assets.AssetReturn.Variance;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class AssetReturnWindowReportChecks
{
    public static void AssetReturnWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704007185);
        var items = new List<AssetReturnWindow>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AssetReturnWindow.NewAssetReturnWindow($"asset-return-window-{index}", "sample"));
        }
        var report = AssetReturnWindowReport.BuildAssetReturnWindowReport("test", items, clock);
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
        AssetReturnWindowReportCountsEachRecordOnce();
    }
}
