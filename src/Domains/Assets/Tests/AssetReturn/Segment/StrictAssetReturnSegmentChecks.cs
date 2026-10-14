namespace Acme.Workforce.Assets.Tests.AssetReturn.Segment;

using Acme.Workforce.Assets.AssetReturn.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class StrictAssetReturnSegmentChecks
{
    public static void StrictAssetReturnSegmentStartsInProposed()
    {
        var item = StrictAssetReturnSegment.NewStrictAssetReturnSegment("code-122", "label-577");
        Check.Equal(item.Status, StrictAssetReturnSegmentStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictAssetReturnSegmentMovesToRejected()
    {
        var item = StrictAssetReturnSegment.NewStrictAssetReturnSegment("code-122", "label-577");
        try
        {
            item.MoveTo(StrictAssetReturnSegmentStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictAssetReturnSegmentStatus.Rejected, "status");
    }

    public static void StrictAssetReturnSegmentRejectsMovesFromWithdrawn()
    {
        var item = StrictAssetReturnSegment.NewStrictAssetReturnSegment("code-122", "label-577");
        item.Status = StrictAssetReturnSegmentStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(StrictAssetReturnSegmentStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        StrictAssetReturnSegmentStartsInProposed();
        StrictAssetReturnSegmentMovesToRejected();
        StrictAssetReturnSegmentRejectsMovesFromWithdrawn();
    }
}
