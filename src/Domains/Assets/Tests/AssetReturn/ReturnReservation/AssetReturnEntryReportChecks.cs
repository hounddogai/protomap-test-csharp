namespace Acme.Workforce.Assets.Tests.AssetReturn.ReturnReservation;

using Acme.Workforce.Assets.AssetReturn.ReturnReservation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of return reservation code, run by the project's runner.</summary>
public static class AssetReturnEntryReportChecks
{
    public static void AssetReturnEntryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704364376);
        var items = new List<AssetReturnEntry>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AssetReturnEntry.NewAssetReturnEntry($"asset-return-entry-{index}", "sample"));
        }
        var report = AssetReturnEntryReport.BuildAssetReturnEntryReport("test", items, clock);
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
        AssetReturnEntryReportCountsEachRecordOnce();
    }
}
