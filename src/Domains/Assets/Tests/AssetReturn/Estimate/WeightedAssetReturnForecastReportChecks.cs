namespace Acme.Workforce.Assets.Tests.AssetReturn.Estimate;

using Acme.Workforce.Assets.AssetReturn.Estimate;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class WeightedAssetReturnForecastReportChecks
{
    public static void WeightedAssetReturnForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703608516);
        var items = new List<WeightedAssetReturnForecast>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(WeightedAssetReturnForecast.NewWeightedAssetReturnForecast($"weighted-asset-return-forecast-{index}", "sample"));
        }
        var report = WeightedAssetReturnForecastReport.BuildWeightedAssetReturnForecastReport("test", items, clock);
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
        WeightedAssetReturnForecastReportCountsEachRecordOnce();
    }
}
