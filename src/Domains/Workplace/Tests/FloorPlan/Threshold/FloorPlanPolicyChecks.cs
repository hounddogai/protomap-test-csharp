namespace Acme.Workforce.Workplace.Tests.FloorPlan.Threshold;

using Acme.Workforce.Workplace.FloorPlan.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class FloorPlanPolicyChecks
{
    public static void FloorPlanPolicyStartsInIdle()
    {
        var item = FloorPlanPolicy.NewFloorPlanPolicy("code-596", "title-501");
        Check.Equal(item.Status, FloorPlanPolicyStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FloorPlanPolicyMovesToWarming()
    {
        var item = FloorPlanPolicy.NewFloorPlanPolicy("code-596", "title-501");
        try
        {
            item.MoveTo(FloorPlanPolicyStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FloorPlanPolicyStatus.Warming, "status");
    }

    public static void FloorPlanPolicyRejectsMovesFromStopped()
    {
        var item = FloorPlanPolicy.NewFloorPlanPolicy("code-596", "title-501");
        item.Status = FloorPlanPolicyStatus.Stopped;
        Check.Fails(() => item.MoveTo(FloorPlanPolicyStatus.Idle), "move");
    }

    public static void RunAll()
    {
        FloorPlanPolicyStartsInIdle();
        FloorPlanPolicyMovesToWarming();
        FloorPlanPolicyRejectsMovesFromStopped();
    }
}
