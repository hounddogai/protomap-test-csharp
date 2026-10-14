namespace Acme.Workforce.Assets.Tests.AssetReturn.Ledger;

using Acme.Workforce.Assets.AssetReturn.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class FallbackAssetReturnThresholdChecks
{
    public static void FallbackAssetReturnThresholdStartsInOpen()
    {
        var item = FallbackAssetReturnThreshold.NewFallbackAssetReturnThreshold("code-354", "label-466");
        Check.Equal(item.Status, FallbackAssetReturnThresholdStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackAssetReturnThresholdMovesToInReview()
    {
        var item = FallbackAssetReturnThreshold.NewFallbackAssetReturnThreshold("code-354", "label-466");
        try
        {
            item.MoveTo(FallbackAssetReturnThresholdStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackAssetReturnThresholdStatus.InReview, "status");
    }

    public static void FallbackAssetReturnThresholdRejectsMovesFromArchived()
    {
        var item = FallbackAssetReturnThreshold.NewFallbackAssetReturnThreshold("code-354", "label-466");
        item.Status = FallbackAssetReturnThresholdStatus.Archived;
        Check.Fails(() => item.MoveTo(FallbackAssetReturnThresholdStatus.Open), "move");
    }

    public static void RunAll()
    {
        FallbackAssetReturnThresholdStartsInOpen();
        FallbackAssetReturnThresholdMovesToInReview();
        FallbackAssetReturnThresholdRejectsMovesFromArchived();
    }
}
