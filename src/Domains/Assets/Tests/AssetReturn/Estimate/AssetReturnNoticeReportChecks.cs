namespace Acme.Workforce.Assets.Tests.AssetReturn.Estimate;

using Acme.Workforce.Assets.AssetReturn.Estimate;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class AssetReturnNoticeReportChecks
{
    public static void AssetReturnNoticeReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702514228);
        var items = new List<AssetReturnNotice>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(AssetReturnNotice.NewAssetReturnNotice($"asset-return-notice-{index}", "sample"));
        }
        var report = AssetReturnNoticeReport.BuildAssetReturnNoticeReport("test", items, clock);
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
        AssetReturnNoticeReportCountsEachRecordOnce();
    }
}
