namespace Acme.Workforce.Workplace.Tests.FloorPlan.Score;

using Acme.Workforce.Workplace.FloorPlan.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class FloorPlanScoreChecks
{
    public static void FloorPlanScoreStartsInIdle()
    {
        var item = FloorPlanScore.NewFloorPlanScore("code-415", "name-513");
        Check.Equal(item.Status, FloorPlanScoreStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FloorPlanScoreMovesToDraining()
    {
        var item = FloorPlanScore.NewFloorPlanScore("code-415", "name-513");
        try
        {
            item.MoveTo(FloorPlanScoreStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FloorPlanScoreStatus.Draining, "status");
    }

    public static void FloorPlanScoreRejectsMovesFromStopped()
    {
        var item = FloorPlanScore.NewFloorPlanScore("code-415", "name-513");
        item.Status = FloorPlanScoreStatus.Stopped;
        Check.Fails(() => item.MoveTo(FloorPlanScoreStatus.Idle), "move");
    }

    public static void RunAll()
    {
        FloorPlanScoreStartsInIdle();
        FloorPlanScoreMovesToDraining();
        FloorPlanScoreRejectsMovesFromStopped();
    }
}
