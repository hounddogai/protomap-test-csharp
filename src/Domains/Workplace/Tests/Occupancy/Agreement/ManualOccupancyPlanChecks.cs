namespace Acme.Workforce.Workplace.Tests.Occupancy.Agreement;

using Acme.Workforce.Workplace.Occupancy.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class ManualOccupancyPlanChecks
{
    public static void ManualOccupancyPlanStartsInNew()
    {
        var item = ManualOccupancyPlan.NewManualOccupancyPlan("code-863", "name-787");
        Check.Equal(item.Status, ManualOccupancyPlanStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualOccupancyPlanMovesToQueued()
    {
        var item = ManualOccupancyPlan.NewManualOccupancyPlan("code-863", "name-787");
        try
        {
            item.MoveTo(ManualOccupancyPlanStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualOccupancyPlanStatus.Queued, "status");
    }

    public static void ManualOccupancyPlanRejectsMovesFromFailed()
    {
        var item = ManualOccupancyPlan.NewManualOccupancyPlan("code-863", "name-787");
        item.Status = ManualOccupancyPlanStatus.Failed;
        Check.Fails(() => item.MoveTo(ManualOccupancyPlanStatus.New), "move");
    }

    public static void RunAll()
    {
        ManualOccupancyPlanStartsInNew();
        ManualOccupancyPlanMovesToQueued();
        ManualOccupancyPlanRejectsMovesFromFailed();
    }
}
