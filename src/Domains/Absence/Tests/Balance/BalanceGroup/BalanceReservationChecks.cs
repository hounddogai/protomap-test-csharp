namespace Acme.Workforce.Absence.Tests.Balance.BalanceGroup;

using Acme.Workforce.Absence.Balance.BalanceGroup;

/// <summary>Checks of balance group code, run by the project's runner.</summary>
public static class BalanceReservationChecks
{
    public static void BalanceReservationStartsInPlanned()
    {
        var item = BalanceReservation.NewBalanceReservation("code-418", "title-341");
        Check.Equal(item.Status, BalanceReservationStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BalanceReservationMovesToScheduled()
    {
        var item = BalanceReservation.NewBalanceReservation("code-418", "title-341");
        try
        {
            item.MoveTo(BalanceReservationStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BalanceReservationStatus.Scheduled, "status");
    }

    public static void BalanceReservationRejectsMovesFromCancelled()
    {
        var item = BalanceReservation.NewBalanceReservation("code-418", "title-341");
        item.Status = BalanceReservationStatus.Cancelled;
        Check.Fails(() => item.MoveTo(BalanceReservationStatus.Planned), "move");
    }

    public static void RunAll()
    {
        BalanceReservationStartsInPlanned();
        BalanceReservationMovesToScheduled();
        BalanceReservationRejectsMovesFromCancelled();
    }
}
