namespace Acme.Workforce.Assets.Tests.AssetReturn.Summary;

using Acme.Workforce.Assets.AssetReturn.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class AssetReturnBaselineChecks
{
    public static void AssetReturnBaselineStartsInOpen()
    {
        var item = AssetReturnBaseline.NewAssetReturnBaseline("code-701", "title-243");
        Check.Equal(item.Status, AssetReturnBaselineStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssetReturnBaselineMovesToInReview()
    {
        var item = AssetReturnBaseline.NewAssetReturnBaseline("code-701", "title-243");
        try
        {
            item.MoveTo(AssetReturnBaselineStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssetReturnBaselineStatus.InReview, "status");
    }

    public static void AssetReturnBaselineRejectsMovesFromArchived()
    {
        var item = AssetReturnBaseline.NewAssetReturnBaseline("code-701", "title-243");
        item.Status = AssetReturnBaselineStatus.Archived;
        Check.Fails(() => item.MoveTo(AssetReturnBaselineStatus.Open), "move");
    }

    public static void RunAll()
    {
        AssetReturnBaselineStartsInOpen();
        AssetReturnBaselineMovesToInReview();
        AssetReturnBaselineRejectsMovesFromArchived();
    }
}
