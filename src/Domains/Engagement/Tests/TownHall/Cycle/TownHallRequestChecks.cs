namespace Acme.Workforce.Engagement.Tests.TownHall.Cycle;

using Acme.Workforce.Engagement.TownHall.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class TownHallRequestChecks
{
    public static void TownHallRequestStartsInPlanned()
    {
        var item = TownHallRequest.NewTownHallRequest("code-219", "label-105");
        Check.Equal(item.Status, TownHallRequestStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TownHallRequestMovesToCompleted()
    {
        var item = TownHallRequest.NewTownHallRequest("code-219", "label-105");
        try
        {
            item.MoveTo(TownHallRequestStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TownHallRequestStatus.Completed, "status");
    }

    public static void TownHallRequestRejectsMovesFromCancelled()
    {
        var item = TownHallRequest.NewTownHallRequest("code-219", "label-105");
        item.Status = TownHallRequestStatus.Cancelled;
        Check.Fails(() => item.MoveTo(TownHallRequestStatus.Planned), "move");
    }

    public static void RunAll()
    {
        TownHallRequestStartsInPlanned();
        TownHallRequestMovesToCompleted();
        TownHallRequestRejectsMovesFromCancelled();
    }
}
