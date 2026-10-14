namespace Acme.Workforce.Talent.Tests.Pool.Plan;

using Acme.Workforce.Talent.Pool.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class ScheduledPoolPlanChecks
{
    public static void ScheduledPoolPlanStartsInIdle()
    {
        var item = ScheduledPoolPlan.NewScheduledPoolPlan("code-541", "name-377");
        Check.Equal(item.Status, ScheduledPoolPlanStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledPoolPlanMovesToWarming()
    {
        var item = ScheduledPoolPlan.NewScheduledPoolPlan("code-541", "name-377");
        try
        {
            item.MoveTo(ScheduledPoolPlanStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledPoolPlanStatus.Warming, "status");
    }

    public static void ScheduledPoolPlanRejectsMovesFromStopped()
    {
        var item = ScheduledPoolPlan.NewScheduledPoolPlan("code-541", "name-377");
        item.Status = ScheduledPoolPlanStatus.Stopped;
        Check.Fails(() => item.MoveTo(ScheduledPoolPlanStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ScheduledPoolPlanStartsInIdle();
        ScheduledPoolPlanMovesToWarming();
        ScheduledPoolPlanRejectsMovesFromStopped();
    }
}
