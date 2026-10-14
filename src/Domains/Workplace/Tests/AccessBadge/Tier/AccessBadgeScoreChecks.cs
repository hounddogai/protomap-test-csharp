namespace Acme.Workforce.Workplace.Tests.AccessBadge.Tier;

using Acme.Workforce.Workplace.AccessBadge.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class AccessBadgeScoreChecks
{
    public static void AccessBadgeScoreStartsInNew()
    {
        var item = AccessBadgeScore.NewAccessBadgeScore("code-922", "name-968");
        Check.Equal(item.Status, AccessBadgeScoreStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccessBadgeScoreMovesToQueued()
    {
        var item = AccessBadgeScore.NewAccessBadgeScore("code-922", "name-968");
        try
        {
            item.MoveTo(AccessBadgeScoreStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccessBadgeScoreStatus.Queued, "status");
    }

    public static void AccessBadgeScoreRejectsMovesFromFailed()
    {
        var item = AccessBadgeScore.NewAccessBadgeScore("code-922", "name-968");
        item.Status = AccessBadgeScoreStatus.Failed;
        Check.Fails(() => item.MoveTo(AccessBadgeScoreStatus.New), "move");
    }

    public static void RunAll()
    {
        AccessBadgeScoreStartsInNew();
        AccessBadgeScoreMovesToQueued();
        AccessBadgeScoreRejectsMovesFromFailed();
    }
}
