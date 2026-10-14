namespace Acme.Workforce.Orgdesign.Tests.Reorg.Snapshot;

using Acme.Workforce.Orgdesign.Reorg.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ReorgScoreChecks
{
    public static void ReorgScoreStartsInOpen()
    {
        var item = ReorgScore.NewReorgScore("code-749", "label-261");
        Check.Equal(item.Status, ReorgScoreStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReorgScoreMovesToResolved()
    {
        var item = ReorgScore.NewReorgScore("code-749", "label-261");
        try
        {
            item.MoveTo(ReorgScoreStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReorgScoreStatus.Resolved, "status");
    }

    public static void ReorgScoreRejectsMovesFromArchived()
    {
        var item = ReorgScore.NewReorgScore("code-749", "label-261");
        item.Status = ReorgScoreStatus.Archived;
        Check.Fails(() => item.MoveTo(ReorgScoreStatus.Open), "move");
    }

    public static void RunAll()
    {
        ReorgScoreStartsInOpen();
        ReorgScoreMovesToResolved();
        ReorgScoreRejectsMovesFromArchived();
    }
}
