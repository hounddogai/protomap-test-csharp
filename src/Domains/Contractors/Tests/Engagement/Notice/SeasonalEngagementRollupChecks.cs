namespace Acme.Workforce.Contractors.Tests.Engagement.Notice;

using Acme.Workforce.Contractors.Engagement.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class SeasonalEngagementRollupChecks
{
    public static void SeasonalEngagementRollupStartsInDraft()
    {
        var item = SeasonalEngagementRollup.NewSeasonalEngagementRollup("code-601", "title-56");
        Check.Equal(item.Status, SeasonalEngagementRollupStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalEngagementRollupMovesToPending()
    {
        var item = SeasonalEngagementRollup.NewSeasonalEngagementRollup("code-601", "title-56");
        try
        {
            item.MoveTo(SeasonalEngagementRollupStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalEngagementRollupStatus.Pending, "status");
    }

    public static void SeasonalEngagementRollupRejectsMovesFromClosed()
    {
        var item = SeasonalEngagementRollup.NewSeasonalEngagementRollup("code-601", "title-56");
        item.Status = SeasonalEngagementRollupStatus.Closed;
        Check.Fails(() => item.MoveTo(SeasonalEngagementRollupStatus.Draft), "move");
    }

    public static void RunAll()
    {
        SeasonalEngagementRollupStartsInDraft();
        SeasonalEngagementRollupMovesToPending();
        SeasonalEngagementRollupRejectsMovesFromClosed();
    }
}
