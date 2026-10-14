namespace Acme.Workforce.Assets.Tests.AssetReturn.Profile;

using Acme.Workforce.Assets.AssetReturn.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AssetReturnForecastChecks
{
    public static void AssetReturnForecastStartsInOpen()
    {
        var item = AssetReturnForecast.NewAssetReturnForecast("code-795", "name-694");
        Check.Equal(item.Status, AssetReturnForecastStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssetReturnForecastMovesToInReview()
    {
        var item = AssetReturnForecast.NewAssetReturnForecast("code-795", "name-694");
        try
        {
            item.MoveTo(AssetReturnForecastStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssetReturnForecastStatus.InReview, "status");
    }

    public static void AssetReturnForecastRejectsMovesFromArchived()
    {
        var item = AssetReturnForecast.NewAssetReturnForecast("code-795", "name-694");
        item.Status = AssetReturnForecastStatus.Archived;
        Check.Fails(() => item.MoveTo(AssetReturnForecastStatus.Open), "move");
    }

    public static void RunAll()
    {
        AssetReturnForecastStartsInOpen();
        AssetReturnForecastMovesToInReview();
        AssetReturnForecastRejectsMovesFromArchived();
    }
}
