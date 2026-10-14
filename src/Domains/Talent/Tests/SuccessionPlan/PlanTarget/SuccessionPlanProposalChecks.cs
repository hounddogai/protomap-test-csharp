namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanTarget;

using Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

/// <summary>Checks of plan target code, run by the project's runner.</summary>
public static class SuccessionPlanProposalChecks
{
    public static void SuccessionPlanProposalStartsInDraft()
    {
        var item = SuccessionPlanProposal.NewSuccessionPlanProposal("code-566", "name-505");
        Check.Equal(item.Status, SuccessionPlanProposalStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SuccessionPlanProposalMovesToPending()
    {
        var item = SuccessionPlanProposal.NewSuccessionPlanProposal("code-566", "name-505");
        try
        {
            item.MoveTo(SuccessionPlanProposalStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SuccessionPlanProposalStatus.Pending, "status");
    }

    public static void SuccessionPlanProposalRejectsMovesFromClosed()
    {
        var item = SuccessionPlanProposal.NewSuccessionPlanProposal("code-566", "name-505");
        item.Status = SuccessionPlanProposalStatus.Closed;
        Check.Fails(() => item.MoveTo(SuccessionPlanProposalStatus.Draft), "move");
    }

    public static void RunAll()
    {
        SuccessionPlanProposalStartsInDraft();
        SuccessionPlanProposalMovesToPending();
        SuccessionPlanProposalRejectsMovesFromClosed();
    }
}
