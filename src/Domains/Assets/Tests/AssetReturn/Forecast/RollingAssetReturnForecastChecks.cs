namespace Acme.Workforce.Assets.Tests.AssetReturn.Forecast;

using Acme.Workforce.Assets.AssetReturn.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class RollingAssetReturnForecastChecks
{
    public static void RollingAssetReturnForecastStartsInNew()
    {
        var item = RollingAssetReturnForecast.NewRollingAssetReturnForecast("code-62", "title-375");
        Check.Equal(item.Status, RollingAssetReturnForecastStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingAssetReturnForecastMovesToRunning()
    {
        var item = RollingAssetReturnForecast.NewRollingAssetReturnForecast("code-62", "title-375");
        try
        {
            item.MoveTo(RollingAssetReturnForecastStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingAssetReturnForecastStatus.Running, "status");
    }

    public static void RollingAssetReturnForecastRejectsMovesFromFailed()
    {
        var item = RollingAssetReturnForecast.NewRollingAssetReturnForecast("code-62", "title-375");
        item.Status = RollingAssetReturnForecastStatus.Failed;
        Check.Fails(() => item.MoveTo(RollingAssetReturnForecastStatus.New), "move");
    }

    public static void RunAll()
    {
        RollingAssetReturnForecastStartsInNew();
        RollingAssetReturnForecastMovesToRunning();
        RollingAssetReturnForecastRejectsMovesFromFailed();
    }
}
