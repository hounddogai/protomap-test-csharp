namespace Acme.Workforce.Orgdesign.Tests.Reorg.Draft;

using Acme.Workforce.Orgdesign.Reorg.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class RegionalReorgScoreChecks
{
    public static void RegionalReorgScoreStartsInOpen()
    {
        var item = RegionalReorgScore.NewRegionalReorgScore("code-22", "name-696");
        Check.Equal(item.Status, RegionalReorgScoreStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalReorgScoreMovesToReopened()
    {
        var item = RegionalReorgScore.NewRegionalReorgScore("code-22", "name-696");
        try
        {
            item.MoveTo(RegionalReorgScoreStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalReorgScoreStatus.Reopened, "status");
    }

    public static void RegionalReorgScoreRejectsMovesFromArchived()
    {
        var item = RegionalReorgScore.NewRegionalReorgScore("code-22", "name-696");
        item.Status = RegionalReorgScoreStatus.Archived;
        Check.Fails(() => item.MoveTo(RegionalReorgScoreStatus.Open), "move");
    }

    public static void RunAll()
    {
        RegionalReorgScoreStartsInOpen();
        RegionalReorgScoreMovesToReopened();
        RegionalReorgScoreRejectsMovesFromArchived();
    }
}
