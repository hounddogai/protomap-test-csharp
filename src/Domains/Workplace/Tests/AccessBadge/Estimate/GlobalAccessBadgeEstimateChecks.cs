namespace Acme.Workforce.Workplace.Tests.AccessBadge.Estimate;

using Acme.Workforce.Workplace.AccessBadge.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class GlobalAccessBadgeEstimateChecks
{
    public static void GlobalAccessBadgeEstimateStartsInIdle()
    {
        var item = GlobalAccessBadgeEstimate.NewGlobalAccessBadgeEstimate("code-482", "name-172");
        Check.Equal(item.Status, GlobalAccessBadgeEstimateStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalAccessBadgeEstimateMovesToReady()
    {
        var item = GlobalAccessBadgeEstimate.NewGlobalAccessBadgeEstimate("code-482", "name-172");
        try
        {
            item.MoveTo(GlobalAccessBadgeEstimateStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalAccessBadgeEstimateStatus.Ready, "status");
    }

    public static void GlobalAccessBadgeEstimateRejectsMovesFromStopped()
    {
        var item = GlobalAccessBadgeEstimate.NewGlobalAccessBadgeEstimate("code-482", "name-172");
        item.Status = GlobalAccessBadgeEstimateStatus.Stopped;
        Check.Fails(() => item.MoveTo(GlobalAccessBadgeEstimateStatus.Idle), "move");
    }

    public static void RunAll()
    {
        GlobalAccessBadgeEstimateStartsInIdle();
        GlobalAccessBadgeEstimateMovesToReady();
        GlobalAccessBadgeEstimateRejectsMovesFromStopped();
    }
}
