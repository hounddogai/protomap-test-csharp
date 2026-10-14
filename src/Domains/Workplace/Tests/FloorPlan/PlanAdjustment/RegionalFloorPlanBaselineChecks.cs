namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanAdjustment;

using Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

/// <summary>Checks of plan adjustment code, run by the project's runner.</summary>
public static class RegionalFloorPlanBaselineChecks
{
    public static void RegionalFloorPlanBaselineStartsInProposed()
    {
        var item = RegionalFloorPlanBaseline.NewRegionalFloorPlanBaseline("code-906", "label-553");
        Check.Equal(item.Status, RegionalFloorPlanBaselineStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalFloorPlanBaselineMovesToRejected()
    {
        var item = RegionalFloorPlanBaseline.NewRegionalFloorPlanBaseline("code-906", "label-553");
        try
        {
            item.MoveTo(RegionalFloorPlanBaselineStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalFloorPlanBaselineStatus.Rejected, "status");
    }

    public static void RegionalFloorPlanBaselineRejectsMovesFromWithdrawn()
    {
        var item = RegionalFloorPlanBaseline.NewRegionalFloorPlanBaseline("code-906", "label-553");
        item.Status = RegionalFloorPlanBaselineStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(RegionalFloorPlanBaselineStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        RegionalFloorPlanBaselineStartsInProposed();
        RegionalFloorPlanBaselineMovesToRejected();
        RegionalFloorPlanBaselineRejectsMovesFromWithdrawn();
    }
}
