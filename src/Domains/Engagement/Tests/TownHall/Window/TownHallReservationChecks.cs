namespace Acme.Workforce.Engagement.Tests.TownHall.Window;

using Acme.Workforce.Engagement.TownHall.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class TownHallReservationChecks
{
    public static void TownHallReservationStartsInNew()
    {
        var item = TownHallReservation.NewTownHallReservation("code-796", "label-370");
        Check.Equal(item.Status, TownHallReservationStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TownHallReservationMovesToQueued()
    {
        var item = TownHallReservation.NewTownHallReservation("code-796", "label-370");
        try
        {
            item.MoveTo(TownHallReservationStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TownHallReservationStatus.Queued, "status");
    }

    public static void TownHallReservationRejectsMovesFromFailed()
    {
        var item = TownHallReservation.NewTownHallReservation("code-796", "label-370");
        item.Status = TownHallReservationStatus.Failed;
        Check.Fails(() => item.MoveTo(TownHallReservationStatus.New), "move");
    }

    public static void RunAll()
    {
        TownHallReservationStartsInNew();
        TownHallReservationMovesToQueued();
        TownHallReservationRejectsMovesFromFailed();
    }
}
