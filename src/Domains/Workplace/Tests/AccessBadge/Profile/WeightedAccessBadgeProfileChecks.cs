namespace Acme.Workforce.Workplace.Tests.AccessBadge.Profile;

using Acme.Workforce.Workplace.AccessBadge.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class WeightedAccessBadgeProfileChecks
{
    public static void WeightedAccessBadgeProfileStartsInOpen()
    {
        var item = WeightedAccessBadgeProfile.NewWeightedAccessBadgeProfile("code-5", "label-380");
        Check.Equal(item.Status, WeightedAccessBadgeProfileStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedAccessBadgeProfileMovesToResolved()
    {
        var item = WeightedAccessBadgeProfile.NewWeightedAccessBadgeProfile("code-5", "label-380");
        try
        {
            item.MoveTo(WeightedAccessBadgeProfileStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedAccessBadgeProfileStatus.Resolved, "status");
    }

    public static void WeightedAccessBadgeProfileRejectsMovesFromArchived()
    {
        var item = WeightedAccessBadgeProfile.NewWeightedAccessBadgeProfile("code-5", "label-380");
        item.Status = WeightedAccessBadgeProfileStatus.Archived;
        Check.Fails(() => item.MoveTo(WeightedAccessBadgeProfileStatus.Open), "move");
    }

    public static void RunAll()
    {
        WeightedAccessBadgeProfileStartsInOpen();
        WeightedAccessBadgeProfileMovesToResolved();
        WeightedAccessBadgeProfileRejectsMovesFromArchived();
    }
}
