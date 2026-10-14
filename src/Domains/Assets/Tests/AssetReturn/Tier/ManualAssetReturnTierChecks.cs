namespace Acme.Workforce.Assets.Tests.AssetReturn.Tier;

using Acme.Workforce.Assets.AssetReturn.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class ManualAssetReturnTierChecks
{
    public static void ManualAssetReturnTierStartsInDraft()
    {
        var item = ManualAssetReturnTier.NewManualAssetReturnTier("code-500", "title-806");
        Check.Equal(item.Status, ManualAssetReturnTierStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualAssetReturnTierMovesToClosed()
    {
        var item = ManualAssetReturnTier.NewManualAssetReturnTier("code-500", "title-806");
        try
        {
            item.MoveTo(ManualAssetReturnTierStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualAssetReturnTierStatus.Closed, "status");
    }

    public static void ManualAssetReturnTierRejectsMovesFromClosed()
    {
        var item = ManualAssetReturnTier.NewManualAssetReturnTier("code-500", "title-806");
        item.Status = ManualAssetReturnTierStatus.Closed;
        Check.Fails(() => item.MoveTo(ManualAssetReturnTierStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ManualAssetReturnTierStartsInDraft();
        ManualAssetReturnTierMovesToClosed();
        ManualAssetReturnTierRejectsMovesFromClosed();
    }
}
