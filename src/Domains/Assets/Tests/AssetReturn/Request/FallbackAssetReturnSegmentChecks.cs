namespace Acme.Workforce.Assets.Tests.AssetReturn.Request;

using Acme.Workforce.Assets.AssetReturn.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class FallbackAssetReturnSegmentChecks
{
    public static void FallbackAssetReturnSegmentStartsInDraft()
    {
        var item = FallbackAssetReturnSegment.NewFallbackAssetReturnSegment("code-403", "title-868");
        Check.Equal(item.Status, FallbackAssetReturnSegmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackAssetReturnSegmentMovesToActive()
    {
        var item = FallbackAssetReturnSegment.NewFallbackAssetReturnSegment("code-403", "title-868");
        try
        {
            item.MoveTo(FallbackAssetReturnSegmentStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackAssetReturnSegmentStatus.Active, "status");
    }

    public static void FallbackAssetReturnSegmentRejectsMovesFromClosed()
    {
        var item = FallbackAssetReturnSegment.NewFallbackAssetReturnSegment("code-403", "title-868");
        item.Status = FallbackAssetReturnSegmentStatus.Closed;
        Check.Fails(() => item.MoveTo(FallbackAssetReturnSegmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        FallbackAssetReturnSegmentStartsInDraft();
        FallbackAssetReturnSegmentMovesToActive();
        FallbackAssetReturnSegmentRejectsMovesFromClosed();
    }
}
