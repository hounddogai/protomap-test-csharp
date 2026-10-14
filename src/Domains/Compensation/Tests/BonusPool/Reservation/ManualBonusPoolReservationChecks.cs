namespace Acme.Workforce.Compensation.Tests.BonusPool.Reservation;

using Acme.Workforce.Compensation.BonusPool.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class ManualBonusPoolReservationChecks
{
    public static void ManualBonusPoolReservationStartsInCreated()
    {
        var item = ManualBonusPoolReservation.NewManualBonusPoolReservation("code-100", "label-683");
        Check.Equal(item.Status, ManualBonusPoolReservationStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualBonusPoolReservationMovesToPublished()
    {
        var item = ManualBonusPoolReservation.NewManualBonusPoolReservation("code-100", "label-683");
        try
        {
            item.MoveTo(ManualBonusPoolReservationStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualBonusPoolReservationStatus.Published, "status");
    }

    public static void ManualBonusPoolReservationRejectsMovesFromRetired()
    {
        var item = ManualBonusPoolReservation.NewManualBonusPoolReservation("code-100", "label-683");
        item.Status = ManualBonusPoolReservationStatus.Retired;
        Check.Fails(() => item.MoveTo(ManualBonusPoolReservationStatus.Created), "move");
    }

    public static void RunAll()
    {
        ManualBonusPoolReservationStartsInCreated();
        ManualBonusPoolReservationMovesToPublished();
        ManualBonusPoolReservationRejectsMovesFromRetired();
    }
}
