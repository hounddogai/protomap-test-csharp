namespace Acme.Workforce.Assets.Tests.AssetReturn.Cycle;

using Acme.Workforce.Assets.AssetReturn.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AdaptiveAssetReturnNoticeChecks
{
    public static void AdaptiveAssetReturnNoticeStartsInCreated()
    {
        var item = AdaptiveAssetReturnNotice.NewAdaptiveAssetReturnNotice("code-647", "title-900");
        Check.Equal(item.Status, AdaptiveAssetReturnNoticeStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveAssetReturnNoticeMovesToPublished()
    {
        var item = AdaptiveAssetReturnNotice.NewAdaptiveAssetReturnNotice("code-647", "title-900");
        try
        {
            item.MoveTo(AdaptiveAssetReturnNoticeStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveAssetReturnNoticeStatus.Published, "status");
    }

    public static void AdaptiveAssetReturnNoticeRejectsMovesFromRetired()
    {
        var item = AdaptiveAssetReturnNotice.NewAdaptiveAssetReturnNotice("code-647", "title-900");
        item.Status = AdaptiveAssetReturnNoticeStatus.Retired;
        Check.Fails(() => item.MoveTo(AdaptiveAssetReturnNoticeStatus.Created), "move");
    }

    public static void RunAll()
    {
        AdaptiveAssetReturnNoticeStartsInCreated();
        AdaptiveAssetReturnNoticeMovesToPublished();
        AdaptiveAssetReturnNoticeRejectsMovesFromRetired();
    }
}
