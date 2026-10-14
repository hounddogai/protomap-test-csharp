namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

/// <summary>Checks of occupancy adjustment code, run by the project's runner.</summary>
public static class AdaptiveOccupancyTemplateChecks
{
    public static void AdaptiveOccupancyTemplateStartsInIdle()
    {
        var item = AdaptiveOccupancyTemplate.NewAdaptiveOccupancyTemplate("code-680", "label-203");
        Check.Equal(item.Status, AdaptiveOccupancyTemplateStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveOccupancyTemplateMovesToReady()
    {
        var item = AdaptiveOccupancyTemplate.NewAdaptiveOccupancyTemplate("code-680", "label-203");
        try
        {
            item.MoveTo(AdaptiveOccupancyTemplateStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveOccupancyTemplateStatus.Ready, "status");
    }

    public static void AdaptiveOccupancyTemplateRejectsMovesFromStopped()
    {
        var item = AdaptiveOccupancyTemplate.NewAdaptiveOccupancyTemplate("code-680", "label-203");
        item.Status = AdaptiveOccupancyTemplateStatus.Stopped;
        Check.Fails(() => item.MoveTo(AdaptiveOccupancyTemplateStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AdaptiveOccupancyTemplateStartsInIdle();
        AdaptiveOccupancyTemplateMovesToReady();
        AdaptiveOccupancyTemplateRejectsMovesFromStopped();
    }
}
