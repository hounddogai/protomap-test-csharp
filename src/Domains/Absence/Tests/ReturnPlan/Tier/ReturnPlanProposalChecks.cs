namespace Acme.Workforce.Absence.Tests.ReturnPlan.Tier;

using Acme.Workforce.Absence.ReturnPlan.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class ReturnPlanProposalChecks
{
    public static void ReturnPlanProposalStartsInProposed()
    {
        var item = ReturnPlanProposal.NewReturnPlanProposal("code-734", "label-371");
        Check.Equal(item.Status, ReturnPlanProposalStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReturnPlanProposalMovesToApproved()
    {
        var item = ReturnPlanProposal.NewReturnPlanProposal("code-734", "label-371");
        try
        {
            item.MoveTo(ReturnPlanProposalStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReturnPlanProposalStatus.Approved, "status");
    }

    public static void ReturnPlanProposalRejectsMovesFromWithdrawn()
    {
        var item = ReturnPlanProposal.NewReturnPlanProposal("code-734", "label-371");
        item.Status = ReturnPlanProposalStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ReturnPlanProposalStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ReturnPlanProposalStartsInProposed();
        ReturnPlanProposalMovesToApproved();
        ReturnPlanProposalRejectsMovesFromWithdrawn();
    }
}
