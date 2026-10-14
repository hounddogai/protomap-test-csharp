namespace Acme.Workforce.Assets.Tests.AssetReturn.Segment;

using Acme.Workforce.Assets.AssetReturn.Segment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class DefaultAssetReturnEstimateReportChecks
{
    public static void DefaultAssetReturnEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707496603);
        var items = new List<DefaultAssetReturnEstimate>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(DefaultAssetReturnEstimate.NewDefaultAssetReturnEstimate($"default-asset-return-estimate-{index}", "sample"));
        }
        var report = DefaultAssetReturnEstimateReport.BuildDefaultAssetReturnEstimateReport("test", items, clock);
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
        DefaultAssetReturnEstimateReportCountsEachRecordOnce();
    }
}
