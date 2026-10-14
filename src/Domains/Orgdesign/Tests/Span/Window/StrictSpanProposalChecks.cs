namespace Acme.Workforce.Orgdesign.Tests.Span.Window;

using Acme.Workforce.Orgdesign.Span.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class StrictSpanProposalChecks
{
    public static void StrictSpanProposalStartsInPlanned()
    {
        var item = StrictSpanProposal.NewStrictSpanProposal("code-465", "title-392");
        Check.Equal(item.Status, StrictSpanProposalStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictSpanProposalMovesToScheduled()
    {
        var item = StrictSpanProposal.NewStrictSpanProposal("code-465", "title-392");
        try
        {
            item.MoveTo(StrictSpanProposalStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictSpanProposalStatus.Scheduled, "status");
    }

    public static void StrictSpanProposalRejectsMovesFromCancelled()
    {
        var item = StrictSpanProposal.NewStrictSpanProposal("code-465", "title-392");
        item.Status = StrictSpanProposalStatus.Cancelled;
        Check.Fails(() => item.MoveTo(StrictSpanProposalStatus.Planned), "move");
    }

    public static void RunAll()
    {
        StrictSpanProposalStartsInPlanned();
        StrictSpanProposalMovesToScheduled();
        StrictSpanProposalRejectsMovesFromCancelled();
    }
}
