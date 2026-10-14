namespace Acme.Workforce.Assets.Tests.AssetReturn.Profile;

using Acme.Workforce.Assets.AssetReturn.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AssetReturnScoreChecks
{
    public static void AssetReturnScoreStartsInCreated()
    {
        var item = AssetReturnScore.NewAssetReturnScore("code-986", "name-337");
        Check.Equal(item.Status, AssetReturnScoreStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssetReturnScoreMovesToValidated()
    {
        var item = AssetReturnScore.NewAssetReturnScore("code-986", "name-337");
        try
        {
            item.MoveTo(AssetReturnScoreStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssetReturnScoreStatus.Validated, "status");
    }

    public static void AssetReturnScoreRejectsMovesFromRetired()
    {
        var item = AssetReturnScore.NewAssetReturnScore("code-986", "name-337");
        item.Status = AssetReturnScoreStatus.Retired;
        Check.Fails(() => item.MoveTo(AssetReturnScoreStatus.Created), "move");
    }

    public static void RunAll()
    {
        AssetReturnScoreStartsInCreated();
        AssetReturnScoreMovesToValidated();
        AssetReturnScoreRejectsMovesFromRetired();
    }
}
