namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeEntry;

using Acme.Workforce.Workplace.AccessBadge.BadgeEntry;

/// <summary>Checks of badge entry code, run by the project's runner.</summary>
public static class RegionalAccessBadgeScoreChecks
{
    public static void RegionalAccessBadgeScoreStartsInCreated()
    {
        var item = RegionalAccessBadgeScore.NewRegionalAccessBadgeScore("code-700", "name-363");
        Check.Equal(item.Status, RegionalAccessBadgeScoreStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalAccessBadgeScoreMovesToValidated()
    {
        var item = RegionalAccessBadgeScore.NewRegionalAccessBadgeScore("code-700", "name-363");
        try
        {
            item.MoveTo(RegionalAccessBadgeScoreStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalAccessBadgeScoreStatus.Validated, "status");
    }

    public static void RegionalAccessBadgeScoreRejectsMovesFromRetired()
    {
        var item = RegionalAccessBadgeScore.NewRegionalAccessBadgeScore("code-700", "name-363");
        item.Status = RegionalAccessBadgeScoreStatus.Retired;
        Check.Fails(() => item.MoveTo(RegionalAccessBadgeScoreStatus.Created), "move");
    }

    public static void RunAll()
    {
        RegionalAccessBadgeScoreStartsInCreated();
        RegionalAccessBadgeScoreMovesToValidated();
        RegionalAccessBadgeScoreRejectsMovesFromRetired();
    }
}
