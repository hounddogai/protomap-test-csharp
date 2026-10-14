namespace Acme.Workforce.Engagement.Tests.ActionPlan.Snapshot;

using Acme.Workforce.Engagement.ActionPlan.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class StrictActionPlanThresholdChecks
{
    public static void StrictActionPlanThresholdStartsInIdle()
    {
        var item = StrictActionPlanThreshold.NewStrictActionPlanThreshold("code-628", "name-645");
        Check.Equal(item.Status, StrictActionPlanThresholdStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictActionPlanThresholdMovesToWarming()
    {
        var item = StrictActionPlanThreshold.NewStrictActionPlanThreshold("code-628", "name-645");
        try
        {
            item.MoveTo(StrictActionPlanThresholdStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictActionPlanThresholdStatus.Warming, "status");
    }

    public static void StrictActionPlanThresholdRejectsMovesFromStopped()
    {
        var item = StrictActionPlanThreshold.NewStrictActionPlanThreshold("code-628", "name-645");
        item.Status = StrictActionPlanThresholdStatus.Stopped;
        Check.Fails(() => item.MoveTo(StrictActionPlanThresholdStatus.Idle), "move");
    }

    public static void RunAll()
    {
        StrictActionPlanThresholdStartsInIdle();
        StrictActionPlanThresholdMovesToWarming();
        StrictActionPlanThresholdRejectsMovesFromStopped();
    }
}
