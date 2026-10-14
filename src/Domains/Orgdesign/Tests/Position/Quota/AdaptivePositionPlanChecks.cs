namespace Acme.Workforce.Orgdesign.Tests.Position.Quota;

using Acme.Workforce.Orgdesign.Position.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class AdaptivePositionPlanChecks
{
    public static void AdaptivePositionPlanStartsInIdle()
    {
        var item = AdaptivePositionPlan.NewAdaptivePositionPlan("code-422", "name-343");
        Check.Equal(item.Status, AdaptivePositionPlanStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptivePositionPlanMovesToWarming()
    {
        var item = AdaptivePositionPlan.NewAdaptivePositionPlan("code-422", "name-343");
        try
        {
            item.MoveTo(AdaptivePositionPlanStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptivePositionPlanStatus.Warming, "status");
    }

    public static void AdaptivePositionPlanRejectsMovesFromStopped()
    {
        var item = AdaptivePositionPlan.NewAdaptivePositionPlan("code-422", "name-343");
        item.Status = AdaptivePositionPlanStatus.Stopped;
        Check.Fails(() => item.MoveTo(AdaptivePositionPlanStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AdaptivePositionPlanStartsInIdle();
        AdaptivePositionPlanMovesToWarming();
        AdaptivePositionPlanRejectsMovesFromStopped();
    }
}
