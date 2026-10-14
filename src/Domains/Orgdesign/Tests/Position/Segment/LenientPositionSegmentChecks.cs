namespace Acme.Workforce.Orgdesign.Tests.Position.Segment;

using Acme.Workforce.Orgdesign.Position.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class LenientPositionSegmentChecks
{
    public static void LenientPositionSegmentStartsInDraft()
    {
        var item = LenientPositionSegment.NewLenientPositionSegment("code-373", "name-139");
        Check.Equal(item.Status, LenientPositionSegmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientPositionSegmentMovesToPending()
    {
        var item = LenientPositionSegment.NewLenientPositionSegment("code-373", "name-139");
        try
        {
            item.MoveTo(LenientPositionSegmentStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientPositionSegmentStatus.Pending, "status");
    }

    public static void LenientPositionSegmentRejectsMovesFromClosed()
    {
        var item = LenientPositionSegment.NewLenientPositionSegment("code-373", "name-139");
        item.Status = LenientPositionSegmentStatus.Closed;
        Check.Fails(() => item.MoveTo(LenientPositionSegmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LenientPositionSegmentStartsInDraft();
        LenientPositionSegmentMovesToPending();
        LenientPositionSegmentRejectsMovesFromClosed();
    }
}
