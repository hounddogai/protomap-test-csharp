namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeEntry;

using Acme.Workforce.Workplace.AccessBadge.BadgeEntry;

/// <summary>Checks of badge entry code, run by the project's runner.</summary>
public static class FallbackAccessBadgeEstimateChecks
{
    public static void FallbackAccessBadgeEstimateStartsInIdle()
    {
        var item = FallbackAccessBadgeEstimate.NewFallbackAccessBadgeEstimate("code-199", "label-265");
        Check.Equal(item.Status, FallbackAccessBadgeEstimateStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackAccessBadgeEstimateMovesToDraining()
    {
        var item = FallbackAccessBadgeEstimate.NewFallbackAccessBadgeEstimate("code-199", "label-265");
        try
        {
            item.MoveTo(FallbackAccessBadgeEstimateStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackAccessBadgeEstimateStatus.Draining, "status");
    }

    public static void FallbackAccessBadgeEstimateRejectsMovesFromStopped()
    {
        var item = FallbackAccessBadgeEstimate.NewFallbackAccessBadgeEstimate("code-199", "label-265");
        item.Status = FallbackAccessBadgeEstimateStatus.Stopped;
        Check.Fails(() => item.MoveTo(FallbackAccessBadgeEstimateStatus.Idle), "move");
    }

    public static void RunAll()
    {
        FallbackAccessBadgeEstimateStartsInIdle();
        FallbackAccessBadgeEstimateMovesToDraining();
        FallbackAccessBadgeEstimateRejectsMovesFromStopped();
    }
}
