namespace Acme.Workforce.Talent.Tests.CareerPath.Plan;

using Acme.Workforce.Talent.CareerPath.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class CareerPathPlanChecks
{
    public static void CareerPathPlanStartsInIdle()
    {
        var item = CareerPathPlan.NewCareerPathPlan("code-260", "label-769");
        Check.Equal(item.Status, CareerPathPlanStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CareerPathPlanMovesToDraining()
    {
        var item = CareerPathPlan.NewCareerPathPlan("code-260", "label-769");
        try
        {
            item.MoveTo(CareerPathPlanStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CareerPathPlanStatus.Draining, "status");
    }

    public static void CareerPathPlanRejectsMovesFromStopped()
    {
        var item = CareerPathPlan.NewCareerPathPlan("code-260", "label-769");
        item.Status = CareerPathPlanStatus.Stopped;
        Check.Fails(() => item.MoveTo(CareerPathPlanStatus.Idle), "move");
    }

    public static void RunAll()
    {
        CareerPathPlanStartsInIdle();
        CareerPathPlanMovesToDraining();
        CareerPathPlanRejectsMovesFromStopped();
    }
}
