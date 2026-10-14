namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyPlan;

using Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

/// <summary>Checks of occupancy plan code, run by the project's runner.</summary>
public static class LenientOccupancyPlanChecks
{
    public static void LenientOccupancyPlanStartsInRequested()
    {
        var item = LenientOccupancyPlan.NewLenientOccupancyPlan("code-479", "title-745");
        Check.Equal(item.Status, LenientOccupancyPlanStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientOccupancyPlanMovesToExpired()
    {
        var item = LenientOccupancyPlan.NewLenientOccupancyPlan("code-479", "title-745");
        try
        {
            item.MoveTo(LenientOccupancyPlanStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientOccupancyPlanStatus.Expired, "status");
    }

    public static void LenientOccupancyPlanRejectsMovesFromExpired()
    {
        var item = LenientOccupancyPlan.NewLenientOccupancyPlan("code-479", "title-745");
        item.Status = LenientOccupancyPlanStatus.Expired;
        Check.Fails(() => item.MoveTo(LenientOccupancyPlanStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LenientOccupancyPlanStartsInRequested();
        LenientOccupancyPlanMovesToExpired();
        LenientOccupancyPlanRejectsMovesFromExpired();
    }
}
