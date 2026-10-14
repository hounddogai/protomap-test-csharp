namespace Acme.Workforce.Assets.Tests.AssetReturn.Agreement;

using Acme.Workforce.Assets.AssetReturn.Agreement;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class AssetReturnGroupReportChecks
{
    public static void AssetReturnGroupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706181140);
        var items = new List<AssetReturnGroup>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AssetReturnGroup.NewAssetReturnGroup($"asset-return-group-{index}", "sample"));
        }
        var report = AssetReturnGroupReport.BuildAssetReturnGroupReport("test", items, clock);
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
        AssetReturnGroupReportCountsEachRecordOnce();
    }
}
