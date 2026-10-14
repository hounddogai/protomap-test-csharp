namespace Acme.Workforce.Assets.Tests.Laptop.Review;

using Acme.Workforce.Assets.Laptop.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class StrictLaptopReservationChecks
{
    public static void StrictLaptopReservationStartsInCreated()
    {
        var item = StrictLaptopReservation.NewStrictLaptopReservation("code-866", "title-127");
        Check.Equal(item.Status, StrictLaptopReservationStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictLaptopReservationMovesToPublished()
    {
        var item = StrictLaptopReservation.NewStrictLaptopReservation("code-866", "title-127");
        try
        {
            item.MoveTo(StrictLaptopReservationStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictLaptopReservationStatus.Published, "status");
    }

    public static void StrictLaptopReservationRejectsMovesFromRetired()
    {
        var item = StrictLaptopReservation.NewStrictLaptopReservation("code-866", "title-127");
        item.Status = StrictLaptopReservationStatus.Retired;
        Check.Fails(() => item.MoveTo(StrictLaptopReservationStatus.Created), "move");
    }

    public static void RunAll()
    {
        StrictLaptopReservationStartsInCreated();
        StrictLaptopReservationMovesToPublished();
        StrictLaptopReservationRejectsMovesFromRetired();
    }
}
