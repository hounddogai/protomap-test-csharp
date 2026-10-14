namespace Acme.Workforce.Assets.Tests.AssetReturn.Rule;

using Acme.Workforce.Assets.AssetReturn.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AdaptiveAssetReturnVarianceChecks
{
    public static void AdaptiveAssetReturnVarianceStartsInDraft()
    {
        var item = AdaptiveAssetReturnVariance.NewAdaptiveAssetReturnVariance("code-870", "name-802");
        Check.Equal(item.Status, AdaptiveAssetReturnVarianceStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveAssetReturnVarianceMovesToSuspended()
    {
        var item = AdaptiveAssetReturnVariance.NewAdaptiveAssetReturnVariance("code-870", "name-802");
        try
        {
            item.MoveTo(AdaptiveAssetReturnVarianceStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveAssetReturnVarianceStatus.Suspended, "status");
    }

    public static void AdaptiveAssetReturnVarianceRejectsMovesFromClosed()
    {
        var item = AdaptiveAssetReturnVariance.NewAdaptiveAssetReturnVariance("code-870", "name-802");
        item.Status = AdaptiveAssetReturnVarianceStatus.Closed;
        Check.Fails(() => item.MoveTo(AdaptiveAssetReturnVarianceStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AdaptiveAssetReturnVarianceStartsInDraft();
        AdaptiveAssetReturnVarianceMovesToSuspended();
        AdaptiveAssetReturnVarianceRejectsMovesFromClosed();
    }
}
