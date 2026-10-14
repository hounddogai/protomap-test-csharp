namespace Acme.Workforce.Workplace.Tests.FloorPlan.Tier;

using Acme.Workforce.Workplace.FloorPlan.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class FloorPlanTierChecks
{
    public static void FloorPlanTierStartsInCreated()
    {
        var item = FloorPlanTier.NewFloorPlanTier("code-200", "name-625");
        Check.Equal(item.Status, FloorPlanTierStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FloorPlanTierMovesToValidated()
    {
        var item = FloorPlanTier.NewFloorPlanTier("code-200", "name-625");
        try
        {
            item.MoveTo(FloorPlanTierStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FloorPlanTierStatus.Validated, "status");
    }

    public static void FloorPlanTierRejectsMovesFromRetired()
    {
        var item = FloorPlanTier.NewFloorPlanTier("code-200", "name-625");
        item.Status = FloorPlanTierStatus.Retired;
        Check.Fails(() => item.MoveTo(FloorPlanTierStatus.Created), "move");
    }

    public static void RunAll()
    {
        FloorPlanTierStartsInCreated();
        FloorPlanTierMovesToValidated();
        FloorPlanTierRejectsMovesFromRetired();
    }
}
