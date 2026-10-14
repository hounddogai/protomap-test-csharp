namespace Acme.Workforce.Assets.Tests.AssetReturn.Budget;

using Acme.Workforce.Assets.AssetReturn.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class AdaptiveAssetReturnDraftChecks
{
    public static void AdaptiveAssetReturnDraftStartsInProposed()
    {
        var item = AdaptiveAssetReturnDraft.NewAdaptiveAssetReturnDraft("code-741", "label-307");
        Check.Equal(item.Status, AdaptiveAssetReturnDraftStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveAssetReturnDraftMovesToApproved()
    {
        var item = AdaptiveAssetReturnDraft.NewAdaptiveAssetReturnDraft("code-741", "label-307");
        try
        {
            item.MoveTo(AdaptiveAssetReturnDraftStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveAssetReturnDraftStatus.Approved, "status");
    }

    public static void AdaptiveAssetReturnDraftRejectsMovesFromWithdrawn()
    {
        var item = AdaptiveAssetReturnDraft.NewAdaptiveAssetReturnDraft("code-741", "label-307");
        item.Status = AdaptiveAssetReturnDraftStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AdaptiveAssetReturnDraftStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AdaptiveAssetReturnDraftStartsInProposed();
        AdaptiveAssetReturnDraftMovesToApproved();
        AdaptiveAssetReturnDraftRejectsMovesFromWithdrawn();
    }
}
