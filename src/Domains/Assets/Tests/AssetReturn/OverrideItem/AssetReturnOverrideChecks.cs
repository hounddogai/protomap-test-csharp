namespace Acme.Workforce.Assets.Tests.AssetReturn.OverrideItem;

using Acme.Workforce.Assets.AssetReturn.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class AssetReturnOverrideChecks
{
    public static void AssetReturnOverrideStartsInProposed()
    {
        var item = AssetReturnOverride.NewAssetReturnOverride("code-52", "label-637");
        Check.Equal(item.Status, AssetReturnOverrideStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssetReturnOverrideMovesToApproved()
    {
        var item = AssetReturnOverride.NewAssetReturnOverride("code-52", "label-637");
        try
        {
            item.MoveTo(AssetReturnOverrideStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssetReturnOverrideStatus.Approved, "status");
    }

    public static void AssetReturnOverrideRejectsMovesFromWithdrawn()
    {
        var item = AssetReturnOverride.NewAssetReturnOverride("code-52", "label-637");
        item.Status = AssetReturnOverrideStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AssetReturnOverrideStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AssetReturnOverrideStartsInProposed();
        AssetReturnOverrideMovesToApproved();
        AssetReturnOverrideRejectsMovesFromWithdrawn();
    }
}
