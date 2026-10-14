namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeRequest;

using Acme.Workforce.Workplace.AccessBadge.BadgeRequest;

/// <summary>Checks of badge request code, run by the project's runner.</summary>
public static class AccessBadgeEstimateChecks
{
    public static void AccessBadgeEstimateStartsInOpen()
    {
        var item = AccessBadgeEstimate.NewAccessBadgeEstimate("code-224", "title-870");
        Check.Equal(item.Status, AccessBadgeEstimateStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccessBadgeEstimateMovesToResolved()
    {
        var item = AccessBadgeEstimate.NewAccessBadgeEstimate("code-224", "title-870");
        try
        {
            item.MoveTo(AccessBadgeEstimateStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccessBadgeEstimateStatus.Resolved, "status");
    }

    public static void AccessBadgeEstimateRejectsMovesFromArchived()
    {
        var item = AccessBadgeEstimate.NewAccessBadgeEstimate("code-224", "title-870");
        item.Status = AccessBadgeEstimateStatus.Archived;
        Check.Fails(() => item.MoveTo(AccessBadgeEstimateStatus.Open), "move");
    }

    public static void RunAll()
    {
        AccessBadgeEstimateStartsInOpen();
        AccessBadgeEstimateMovesToResolved();
        AccessBadgeEstimateRejectsMovesFromArchived();
    }
}
