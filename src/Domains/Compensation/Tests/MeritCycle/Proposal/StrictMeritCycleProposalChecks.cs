namespace Acme.Workforce.Compensation.Tests.MeritCycle.Proposal;

using Acme.Workforce.Compensation.MeritCycle.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class StrictMeritCycleProposalChecks
{
    public static void StrictMeritCycleProposalStartsInRequested()
    {
        var item = StrictMeritCycleProposal.NewStrictMeritCycleProposal("code-434", "name-96");
        Check.Equal(item.Status, StrictMeritCycleProposalStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictMeritCycleProposalMovesToGranted()
    {
        var item = StrictMeritCycleProposal.NewStrictMeritCycleProposal("code-434", "name-96");
        try
        {
            item.MoveTo(StrictMeritCycleProposalStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictMeritCycleProposalStatus.Granted, "status");
    }

    public static void StrictMeritCycleProposalRejectsMovesFromExpired()
    {
        var item = StrictMeritCycleProposal.NewStrictMeritCycleProposal("code-434", "name-96");
        item.Status = StrictMeritCycleProposalStatus.Expired;
        Check.Fails(() => item.MoveTo(StrictMeritCycleProposalStatus.Requested), "move");
    }

    public static void RunAll()
    {
        StrictMeritCycleProposalStartsInRequested();
        StrictMeritCycleProposalMovesToGranted();
        StrictMeritCycleProposalRejectsMovesFromExpired();
    }
}
