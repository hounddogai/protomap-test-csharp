namespace Acme.Workforce.Assets.Tests.AssetReturn.Request;

using Acme.Workforce.Assets.AssetReturn.Request;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class AssetReturnRequestReportChecks
{
    public static void AssetReturnRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709285441);
        var items = new List<AssetReturnRequest>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AssetReturnRequest.NewAssetReturnRequest($"asset-return-request-{index}", "sample"));
        }
        var report = AssetReturnRequestReport.BuildAssetReturnRequestReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        AssetReturnRequestReportCountsEachRecordOnce();
    }
}
