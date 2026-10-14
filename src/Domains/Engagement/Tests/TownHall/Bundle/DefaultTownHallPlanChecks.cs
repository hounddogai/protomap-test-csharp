namespace Acme.Workforce.Engagement.Tests.TownHall.Bundle;

using Acme.Workforce.Engagement.TownHall.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class DefaultTownHallPlanChecks
{
    public static void DefaultTownHallPlanStartsInPlanned()
    {
        var item = DefaultTownHallPlan.NewDefaultTownHallPlan("code-686", "name-724");
        Check.Equal(item.Status, DefaultTownHallPlanStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultTownHallPlanMovesToCompleted()
    {
        var item = DefaultTownHallPlan.NewDefaultTownHallPlan("code-686", "name-724");
        try
        {
            item.MoveTo(DefaultTownHallPlanStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultTownHallPlanStatus.Completed, "status");
    }

    public static void DefaultTownHallPlanRejectsMovesFromCancelled()
    {
        var item = DefaultTownHallPlan.NewDefaultTownHallPlan("code-686", "name-724");
        item.Status = DefaultTownHallPlanStatus.Cancelled;
        Check.Fails(() => item.MoveTo(DefaultTownHallPlanStatus.Planned), "move");
    }

    public static void RunAll()
    {
        DefaultTownHallPlanStartsInPlanned();
        DefaultTownHallPlanMovesToCompleted();
        DefaultTownHallPlanRejectsMovesFromCancelled();
    }
}
