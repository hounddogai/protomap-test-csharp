namespace Acme.Workforce.Compensation.Tests.BonusPool.Group;

using Acme.Workforce.Compensation.BonusPool.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class BonusPoolReservationChecks
{
    public static void BonusPoolReservationStartsInIdle()
    {
        var item = BonusPoolReservation.NewBonusPoolReservation("code-184", "label-634");
        Check.Equal(item.Status, BonusPoolReservationStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BonusPoolReservationMovesToDraining()
    {
        var item = BonusPoolReservation.NewBonusPoolReservation("code-184", "label-634");
        try
        {
            item.MoveTo(BonusPoolReservationStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BonusPoolReservationStatus.Draining, "status");
    }

    public static void BonusPoolReservationRejectsMovesFromStopped()
    {
        var item = BonusPoolReservation.NewBonusPoolReservation("code-184", "label-634");
        item.Status = BonusPoolReservationStatus.Stopped;
        Check.Fails(() => item.MoveTo(BonusPoolReservationStatus.Idle), "move");
    }

    public static void RunAll()
    {
        BonusPoolReservationStartsInIdle();
        BonusPoolReservationMovesToDraining();
        BonusPoolReservationRejectsMovesFromStopped();
    }
}
