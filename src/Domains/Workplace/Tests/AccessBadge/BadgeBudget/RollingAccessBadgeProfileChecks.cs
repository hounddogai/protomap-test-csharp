namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeBudget;

using Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

/// <summary>Checks of badge budget code, run by the project's runner.</summary>
public static class RollingAccessBadgeProfileChecks
{
    public static void RollingAccessBadgeProfileStartsInOpen()
    {
        var item = RollingAccessBadgeProfile.NewRollingAccessBadgeProfile("code-54", "label-151");
        Check.Equal(item.Status, RollingAccessBadgeProfileStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingAccessBadgeProfileMovesToInReview()
    {
        var item = RollingAccessBadgeProfile.NewRollingAccessBadgeProfile("code-54", "label-151");
        try
        {
            item.MoveTo(RollingAccessBadgeProfileStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingAccessBadgeProfileStatus.InReview, "status");
    }

    public static void RollingAccessBadgeProfileRejectsMovesFromArchived()
    {
        var item = RollingAccessBadgeProfile.NewRollingAccessBadgeProfile("code-54", "label-151");
        item.Status = RollingAccessBadgeProfileStatus.Archived;
        Check.Fails(() => item.MoveTo(RollingAccessBadgeProfileStatus.Open), "move");
    }

    public static void RunAll()
    {
        RollingAccessBadgeProfileStartsInOpen();
        RollingAccessBadgeProfileMovesToInReview();
        RollingAccessBadgeProfileRejectsMovesFromArchived();
    }
}
