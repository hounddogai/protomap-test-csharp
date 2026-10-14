namespace Acme.Workforce.Assets.Tests.Laptop.Review;

using Acme.Workforce.Assets.Laptop.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class StrictLaptopReservationViewChecks
{
    public static void StrictLaptopReservationViewShowsTheStatusInCapitals()
    {
        var item = StrictLaptopReservation.NewStrictLaptopReservation("code-866", "title-127");
        var view = StrictLaptopReservationView.ToStrictLaptopReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictLaptopReservationView.ToStrictLaptopReservationViews(new List<StrictLaptopReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictLaptopReservationViewShowsTheStatusInCapitals();
    }
}
