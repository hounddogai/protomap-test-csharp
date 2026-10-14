namespace Acme.Workforce.Assets.Tests.AssetReturn.Variance;

using Acme.Workforce.Assets.AssetReturn.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class AssetReturnBundleChecks
{
    public static void AssetReturnBundleStartsInNew()
    {
        var item = AssetReturnBundle.NewAssetReturnBundle("code-188", "name-628");
        Check.Equal(item.Status, AssetReturnBundleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssetReturnBundleMovesToQueued()
    {
        var item = AssetReturnBundle.NewAssetReturnBundle("code-188", "name-628");
        try
        {
            item.MoveTo(AssetReturnBundleStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssetReturnBundleStatus.Queued, "status");
    }

    public static void AssetReturnBundleRejectsMovesFromFailed()
    {
        var item = AssetReturnBundle.NewAssetReturnBundle("code-188", "name-628");
        item.Status = AssetReturnBundleStatus.Failed;
        Check.Fails(() => item.MoveTo(AssetReturnBundleStatus.New), "move");
    }

    public static void RunAll()
    {
        AssetReturnBundleStartsInNew();
        AssetReturnBundleMovesToQueued();
        AssetReturnBundleRejectsMovesFromFailed();
    }
}
