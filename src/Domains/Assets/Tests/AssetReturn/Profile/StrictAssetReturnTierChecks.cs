namespace Acme.Workforce.Assets.Tests.AssetReturn.Profile;

using Acme.Workforce.Assets.AssetReturn.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class StrictAssetReturnTierChecks
{
    public static void StrictAssetReturnTierStartsInDraft()
    {
        var item = StrictAssetReturnTier.NewStrictAssetReturnTier("code-492", "label-186");
        Check.Equal(item.Status, StrictAssetReturnTierStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictAssetReturnTierMovesToPending()
    {
        var item = StrictAssetReturnTier.NewStrictAssetReturnTier("code-492", "label-186");
        try
        {
            item.MoveTo(StrictAssetReturnTierStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictAssetReturnTierStatus.Pending, "status");
    }

    public static void StrictAssetReturnTierRejectsMovesFromClosed()
    {
        var item = StrictAssetReturnTier.NewStrictAssetReturnTier("code-492", "label-186");
        item.Status = StrictAssetReturnTierStatus.Closed;
        Check.Fails(() => item.MoveTo(StrictAssetReturnTierStatus.Draft), "move");
    }

    public static void RunAll()
    {
        StrictAssetReturnTierStartsInDraft();
        StrictAssetReturnTierMovesToPending();
        StrictAssetReturnTierRejectsMovesFromClosed();
    }
}
