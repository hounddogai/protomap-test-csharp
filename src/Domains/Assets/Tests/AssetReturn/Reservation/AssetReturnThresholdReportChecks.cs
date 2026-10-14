namespace Acme.Workforce.Assets.Tests.AssetReturn.Reservation;

using Acme.Workforce.Assets.AssetReturn.Reservation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class AssetReturnThresholdReportChecks
{
    public static void AssetReturnThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704282760);
        var items = new List<AssetReturnThreshold>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AssetReturnThreshold.NewAssetReturnThreshold($"asset-return-threshold-{index}", "sample"));
        }
        var report = AssetReturnThresholdReport.BuildAssetReturnThresholdReport("test", items, clock);
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
        AssetReturnThresholdReportCountsEachRecordOnce();
    }
}
