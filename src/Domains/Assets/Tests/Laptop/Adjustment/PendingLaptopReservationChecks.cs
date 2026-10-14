namespace Acme.Workforce.Assets.Tests.Laptop.Adjustment;

using Acme.Workforce.Assets.Laptop.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class PendingLaptopReservationChecks
{
    public static void PendingLaptopReservationStartsInIdle()
    {
        var item = PendingLaptopReservation.NewPendingLaptopReservation("code-199", "title-197");
        Check.Equal(item.Status, PendingLaptopReservationStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingLaptopReservationMovesToWarming()
    {
        var item = PendingLaptopReservation.NewPendingLaptopReservation("code-199", "title-197");
        try
        {
            item.MoveTo(PendingLaptopReservationStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingLaptopReservationStatus.Warming, "status");
    }

    public static void PendingLaptopReservationRejectsMovesFromStopped()
    {
        var item = PendingLaptopReservation.NewPendingLaptopReservation("code-199", "title-197");
        item.Status = PendingLaptopReservationStatus.Stopped;
        Check.Fails(() => item.MoveTo(PendingLaptopReservationStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PendingLaptopReservationStartsInIdle();
        PendingLaptopReservationMovesToWarming();
        PendingLaptopReservationRejectsMovesFromStopped();
    }
}
