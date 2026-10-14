namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeLedger;

using Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

/// <summary>Checks of badge ledger code, run by the project's runner.</summary>
public static class SeasonalAccessBadgeDraftChecks
{
    public static void SeasonalAccessBadgeDraftStartsInOpen()
    {
        var item = SeasonalAccessBadgeDraft.NewSeasonalAccessBadgeDraft("code-642", "title-717");
        Check.Equal(item.Status, SeasonalAccessBadgeDraftStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalAccessBadgeDraftMovesToReopened()
    {
        var item = SeasonalAccessBadgeDraft.NewSeasonalAccessBadgeDraft("code-642", "title-717");
        try
        {
            item.MoveTo(SeasonalAccessBadgeDraftStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalAccessBadgeDraftStatus.Reopened, "status");
    }

    public static void SeasonalAccessBadgeDraftRejectsMovesFromArchived()
    {
        var item = SeasonalAccessBadgeDraft.NewSeasonalAccessBadgeDraft("code-642", "title-717");
        item.Status = SeasonalAccessBadgeDraftStatus.Archived;
        Check.Fails(() => item.MoveTo(SeasonalAccessBadgeDraftStatus.Open), "move");
    }

    public static void RunAll()
    {
        SeasonalAccessBadgeDraftStartsInOpen();
        SeasonalAccessBadgeDraftMovesToReopened();
        SeasonalAccessBadgeDraftRejectsMovesFromArchived();
    }
}
