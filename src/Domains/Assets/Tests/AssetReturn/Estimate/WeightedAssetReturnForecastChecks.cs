namespace Acme.Workforce.Assets.Tests.AssetReturn.Estimate;

using Acme.Workforce.Assets.AssetReturn.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class WeightedAssetReturnForecastChecks
{
    public static void WeightedAssetReturnForecastStartsInIdle()
    {
        var item = WeightedAssetReturnForecast.NewWeightedAssetReturnForecast("code-30", "name-731");
        Check.Equal(item.Status, WeightedAssetReturnForecastStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedAssetReturnForecastMovesToWarming()
    {
        var item = WeightedAssetReturnForecast.NewWeightedAssetReturnForecast("code-30", "name-731");
        try
        {
            item.MoveTo(WeightedAssetReturnForecastStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedAssetReturnForecastStatus.Warming, "status");
    }

    public static void WeightedAssetReturnForecastRejectsMovesFromStopped()
    {
        var item = WeightedAssetReturnForecast.NewWeightedAssetReturnForecast("code-30", "name-731");
        item.Status = WeightedAssetReturnForecastStatus.Stopped;
        Check.Fails(() => item.MoveTo(WeightedAssetReturnForecastStatus.Idle), "move");
    }

    public static void RunAll()
    {
        WeightedAssetReturnForecastStartsInIdle();
        WeightedAssetReturnForecastMovesToWarming();
        WeightedAssetReturnForecastRejectsMovesFromStopped();
    }
}
