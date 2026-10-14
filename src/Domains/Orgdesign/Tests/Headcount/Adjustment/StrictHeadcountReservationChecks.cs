namespace Acme.Workforce.Orgdesign.Tests.Headcount.Adjustment;

using Acme.Workforce.Orgdesign.Headcount.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class StrictHeadcountReservationChecks
{
    public static void StrictHeadcountReservationStartsInIdle()
    {
        var item = StrictHeadcountReservation.NewStrictHeadcountReservation("code-708", "label-448");
        Check.Equal(item.Status, StrictHeadcountReservationStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictHeadcountReservationMovesToReady()
    {
        var item = StrictHeadcountReservation.NewStrictHeadcountReservation("code-708", "label-448");
        try
        {
            item.MoveTo(StrictHeadcountReservationStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictHeadcountReservationStatus.Ready, "status");
    }

    public static void StrictHeadcountReservationRejectsMovesFromStopped()
    {
        var item = StrictHeadcountReservation.NewStrictHeadcountReservation("code-708", "label-448");
        item.Status = StrictHeadcountReservationStatus.Stopped;
        Check.Fails(() => item.MoveTo(StrictHeadcountReservationStatus.Idle), "move");
    }

    public static void RunAll()
    {
        StrictHeadcountReservationStartsInIdle();
        StrictHeadcountReservationMovesToReady();
        StrictHeadcountReservationRejectsMovesFromStopped();
    }
}
