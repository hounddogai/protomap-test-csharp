namespace Acme.Workforce.Assets.Tests.AssetReturn.Summary;

using Acme.Workforce.Assets.AssetReturn.Summary;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class AssetReturnBaselineReportChecks
{
    public static void AssetReturnBaselineReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708001000);
        var items = new List<AssetReturnBaseline>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(AssetReturnBaseline.NewAssetReturnBaseline($"asset-return-baseline-{index}", "sample"));
        }
        var report = AssetReturnBaselineReport.BuildAssetReturnBaselineReport("test", items, clock);
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
        AssetReturnBaselineReportCountsEachRecordOnce();
    }
}
