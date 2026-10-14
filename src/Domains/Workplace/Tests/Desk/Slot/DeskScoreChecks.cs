namespace Acme.Workforce.Workplace.Tests.Desk.Slot;

using Acme.Workforce.Workplace.Desk.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class DeskScoreChecks
{
    public static void DeskScoreStartsInNew()
    {
        var item = DeskScore.NewDeskScore("code-769", "title-861");
        Check.Equal(item.Status, DeskScoreStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeskScoreMovesToQueued()
    {
        var item = DeskScore.NewDeskScore("code-769", "title-861");
        try
        {
            item.MoveTo(DeskScoreStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeskScoreStatus.Queued, "status");
    }

    public static void DeskScoreRejectsMovesFromFailed()
    {
        var item = DeskScore.NewDeskScore("code-769", "title-861");
        item.Status = DeskScoreStatus.Failed;
        Check.Fails(() => item.MoveTo(DeskScoreStatus.New), "move");
    }

    public static void RunAll()
    {
        DeskScoreStartsInNew();
        DeskScoreMovesToQueued();
        DeskScoreRejectsMovesFromFailed();
    }
}
