namespace Acme.Workforce.Orgdesign.Tests.Position.Plan;

using Acme.Workforce.Orgdesign.Position.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class LegacyPositionPlanChecks
{
    public static void LegacyPositionPlanStartsInIdle()
    {
        var item = LegacyPositionPlan.NewLegacyPositionPlan("code-610", "name-157");
        Check.Equal(item.Status, LegacyPositionPlanStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyPositionPlanMovesToWarming()
    {
        var item = LegacyPositionPlan.NewLegacyPositionPlan("code-610", "name-157");
        try
        {
            item.MoveTo(LegacyPositionPlanStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyPositionPlanStatus.Warming, "status");
    }

    public static void LegacyPositionPlanRejectsMovesFromStopped()
    {
        var item = LegacyPositionPlan.NewLegacyPositionPlan("code-610", "name-157");
        item.Status = LegacyPositionPlanStatus.Stopped;
        Check.Fails(() => item.MoveTo(LegacyPositionPlanStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LegacyPositionPlanStartsInIdle();
        LegacyPositionPlanMovesToWarming();
        LegacyPositionPlanRejectsMovesFromStopped();
    }
}
