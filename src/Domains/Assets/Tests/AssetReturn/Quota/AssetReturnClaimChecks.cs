namespace Acme.Workforce.Assets.Tests.AssetReturn.Quota;

using Acme.Workforce.Assets.AssetReturn.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class AssetReturnClaimChecks
{
    public static void AssetReturnClaimStartsInNew()
    {
        var item = AssetReturnClaim.NewAssetReturnClaim("code-707", "label-816");
        Check.Equal(item.Status, AssetReturnClaimStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssetReturnClaimMovesToQueued()
    {
        var item = AssetReturnClaim.NewAssetReturnClaim("code-707", "label-816");
        try
        {
            item.MoveTo(AssetReturnClaimStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssetReturnClaimStatus.Queued, "status");
    }

    public static void AssetReturnClaimRejectsMovesFromFailed()
    {
        var item = AssetReturnClaim.NewAssetReturnClaim("code-707", "label-816");
        item.Status = AssetReturnClaimStatus.Failed;
        Check.Fails(() => item.MoveTo(AssetReturnClaimStatus.New), "move");
    }

    public static void RunAll()
    {
        AssetReturnClaimStartsInNew();
        AssetReturnClaimMovesToQueued();
        AssetReturnClaimRejectsMovesFromFailed();
    }
}
