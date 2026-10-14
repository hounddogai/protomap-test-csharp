namespace Acme.Workforce.Assets.Tests.AssetReturn.OverrideItem;

using Acme.Workforce.Assets.AssetReturn.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class AssetReturnTierChecks
{
    public static void AssetReturnTierStartsInProposed()
    {
        var item = AssetReturnTier.NewAssetReturnTier("code-151", "label-997");
        Check.Equal(item.Status, AssetReturnTierStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssetReturnTierMovesToApproved()
    {
        var item = AssetReturnTier.NewAssetReturnTier("code-151", "label-997");
        try
        {
            item.MoveTo(AssetReturnTierStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssetReturnTierStatus.Approved, "status");
    }

    public static void AssetReturnTierRejectsMovesFromWithdrawn()
    {
        var item = AssetReturnTier.NewAssetReturnTier("code-151", "label-997");
        item.Status = AssetReturnTierStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AssetReturnTierStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AssetReturnTierStartsInProposed();
        AssetReturnTierMovesToApproved();
        AssetReturnTierRejectsMovesFromWithdrawn();
    }
}
