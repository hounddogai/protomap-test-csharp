namespace Acme.Workforce.Absence.Tests.ReturnPlan.PlanAudit;

using Acme.Workforce.Absence.ReturnPlan.PlanAudit;

/// <summary>Checks of plan audit code, run by the project's runner.</summary>
public static class WeightedReturnPlanOverrideChecks
{
    public static void WeightedReturnPlanOverrideStartsInProposed()
    {
        var item = WeightedReturnPlanOverride.NewWeightedReturnPlanOverride("code-322", "label-710");
        Check.Equal(item.Status, WeightedReturnPlanOverrideStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedReturnPlanOverrideMovesToApproved()
    {
        var item = WeightedReturnPlanOverride.NewWeightedReturnPlanOverride("code-322", "label-710");
        try
        {
            item.MoveTo(WeightedReturnPlanOverrideStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedReturnPlanOverrideStatus.Approved, "status");
    }

    public static void WeightedReturnPlanOverrideRejectsMovesFromWithdrawn()
    {
        var item = WeightedReturnPlanOverride.NewWeightedReturnPlanOverride("code-322", "label-710");
        item.Status = WeightedReturnPlanOverrideStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(WeightedReturnPlanOverrideStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        WeightedReturnPlanOverrideStartsInProposed();
        WeightedReturnPlanOverrideMovesToApproved();
        WeightedReturnPlanOverrideRejectsMovesFromWithdrawn();
    }
}
