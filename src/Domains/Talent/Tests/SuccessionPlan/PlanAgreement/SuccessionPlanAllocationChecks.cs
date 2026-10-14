namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanAgreement;

using Acme.Workforce.Talent.SuccessionPlan.PlanAgreement;

/// <summary>Checks of plan agreement code, run by the project's runner.</summary>
public static class SuccessionPlanAllocationChecks
{
    public static void SuccessionPlanAllocationStartsInOpen()
    {
        var item = SuccessionPlanAllocation.NewSuccessionPlanAllocation("code-93", "label-548");
        Check.Equal(item.Status, SuccessionPlanAllocationStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SuccessionPlanAllocationMovesToResolved()
    {
        var item = SuccessionPlanAllocation.NewSuccessionPlanAllocation("code-93", "label-548");
        try
        {
            item.MoveTo(SuccessionPlanAllocationStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SuccessionPlanAllocationStatus.Resolved, "status");
    }

    public static void SuccessionPlanAllocationRejectsMovesFromArchived()
    {
        var item = SuccessionPlanAllocation.NewSuccessionPlanAllocation("code-93", "label-548");
        item.Status = SuccessionPlanAllocationStatus.Archived;
        Check.Fails(() => item.MoveTo(SuccessionPlanAllocationStatus.Open), "move");
    }

    public static void RunAll()
    {
        SuccessionPlanAllocationStartsInOpen();
        SuccessionPlanAllocationMovesToResolved();
        SuccessionPlanAllocationRejectsMovesFromArchived();
    }
}
