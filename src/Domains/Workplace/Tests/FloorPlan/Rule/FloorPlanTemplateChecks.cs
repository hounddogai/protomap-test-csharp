namespace Acme.Workforce.Workplace.Tests.FloorPlan.Rule;

using Acme.Workforce.Workplace.FloorPlan.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class FloorPlanTemplateChecks
{
    public static void FloorPlanTemplateStartsInRequested()
    {
        var item = FloorPlanTemplate.NewFloorPlanTemplate("code-690", "label-680");
        Check.Equal(item.Status, FloorPlanTemplateStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FloorPlanTemplateMovesToRevoked()
    {
        var item = FloorPlanTemplate.NewFloorPlanTemplate("code-690", "label-680");
        try
        {
            item.MoveTo(FloorPlanTemplateStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FloorPlanTemplateStatus.Revoked, "status");
    }

    public static void FloorPlanTemplateRejectsMovesFromExpired()
    {
        var item = FloorPlanTemplate.NewFloorPlanTemplate("code-690", "label-680");
        item.Status = FloorPlanTemplateStatus.Expired;
        Check.Fails(() => item.MoveTo(FloorPlanTemplateStatus.Requested), "move");
    }

    public static void RunAll()
    {
        FloorPlanTemplateStartsInRequested();
        FloorPlanTemplateMovesToRevoked();
        FloorPlanTemplateRejectsMovesFromExpired();
    }
}
