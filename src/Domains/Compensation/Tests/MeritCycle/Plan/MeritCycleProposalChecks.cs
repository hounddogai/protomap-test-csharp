namespace Acme.Workforce.Compensation.Tests.MeritCycle.Plan;

using Acme.Workforce.Compensation.MeritCycle.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class MeritCycleProposalChecks
{
    public static void MeritCycleProposalStartsInNew()
    {
        var item = MeritCycleProposal.NewMeritCycleProposal("code-480", "name-816");
        Check.Equal(item.Status, MeritCycleProposalStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleProposalMovesToQueued()
    {
        var item = MeritCycleProposal.NewMeritCycleProposal("code-480", "name-816");
        try
        {
            item.MoveTo(MeritCycleProposalStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleProposalStatus.Queued, "status");
    }

    public static void MeritCycleProposalRejectsMovesFromFailed()
    {
        var item = MeritCycleProposal.NewMeritCycleProposal("code-480", "name-816");
        item.Status = MeritCycleProposalStatus.Failed;
        Check.Fails(() => item.MoveTo(MeritCycleProposalStatus.New), "move");
    }

    public static void RunAll()
    {
        MeritCycleProposalStartsInNew();
        MeritCycleProposalMovesToQueued();
        MeritCycleProposalRejectsMovesFromFailed();
    }
}
