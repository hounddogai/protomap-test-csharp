namespace Acme.Workforce.Talent.Tests.Nomination.Segment;

using Acme.Workforce.Talent.Nomination.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class NominationSegmentChecks
{
    public static void NominationSegmentStartsInDraft()
    {
        var item = NominationSegment.NewNominationSegment("code-276", "title-757");
        Check.Equal(item.Status, NominationSegmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void NominationSegmentMovesToClosed()
    {
        var item = NominationSegment.NewNominationSegment("code-276", "title-757");
        try
        {
            item.MoveTo(NominationSegmentStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, NominationSegmentStatus.Closed, "status");
    }

    public static void NominationSegmentRejectsMovesFromClosed()
    {
        var item = NominationSegment.NewNominationSegment("code-276", "title-757");
        item.Status = NominationSegmentStatus.Closed;
        Check.Fails(() => item.MoveTo(NominationSegmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        NominationSegmentStartsInDraft();
        NominationSegmentMovesToClosed();
        NominationSegmentRejectsMovesFromClosed();
    }
}
