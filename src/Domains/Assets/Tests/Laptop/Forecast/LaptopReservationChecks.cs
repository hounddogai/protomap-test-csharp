namespace Acme.Workforce.Assets.Tests.Laptop.Forecast;

using Acme.Workforce.Assets.Laptop.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class LaptopReservationChecks
{
    public static void LaptopReservationStartsInProposed()
    {
        var item = LaptopReservation.NewLaptopReservation("code-671", "label-661");
        Check.Equal(item.Status, LaptopReservationStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LaptopReservationMovesToWithdrawn()
    {
        var item = LaptopReservation.NewLaptopReservation("code-671", "label-661");
        try
        {
            item.MoveTo(LaptopReservationStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LaptopReservationStatus.Withdrawn, "status");
    }

    public static void LaptopReservationRejectsMovesFromWithdrawn()
    {
        var item = LaptopReservation.NewLaptopReservation("code-671", "label-661");
        item.Status = LaptopReservationStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LaptopReservationStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LaptopReservationStartsInProposed();
        LaptopReservationMovesToWithdrawn();
        LaptopReservationRejectsMovesFromWithdrawn();
    }
}
