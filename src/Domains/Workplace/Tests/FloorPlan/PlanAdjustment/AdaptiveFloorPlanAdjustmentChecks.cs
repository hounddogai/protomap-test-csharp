namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanAdjustment;

using Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

/// <summary>Checks of plan adjustment code, run by the project's runner.</summary>
public static class AdaptiveFloorPlanAdjustmentChecks
{
    public static void AdaptiveFloorPlanAdjustmentStartsInCreated()
    {
        var item = AdaptiveFloorPlanAdjustment.NewAdaptiveFloorPlanAdjustment("code-995", "label-81");
        Check.Equal(item.Status, AdaptiveFloorPlanAdjustmentStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveFloorPlanAdjustmentMovesToValidated()
    {
        var item = AdaptiveFloorPlanAdjustment.NewAdaptiveFloorPlanAdjustment("code-995", "label-81");
        try
        {
            item.MoveTo(AdaptiveFloorPlanAdjustmentStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveFloorPlanAdjustmentStatus.Validated, "status");
    }

    public static void AdaptiveFloorPlanAdjustmentRejectsMovesFromRetired()
    {
        var item = AdaptiveFloorPlanAdjustment.NewAdaptiveFloorPlanAdjustment("code-995", "label-81");
        item.Status = AdaptiveFloorPlanAdjustmentStatus.Retired;
        Check.Fails(() => item.MoveTo(AdaptiveFloorPlanAdjustmentStatus.Created), "move");
    }

    public static void RunAll()
    {
        AdaptiveFloorPlanAdjustmentStartsInCreated();
        AdaptiveFloorPlanAdjustmentMovesToValidated();
        AdaptiveFloorPlanAdjustmentRejectsMovesFromRetired();
    }
}
