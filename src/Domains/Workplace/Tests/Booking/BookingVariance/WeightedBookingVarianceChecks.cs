namespace Acme.Workforce.Workplace.Tests.Booking.BookingVariance;

using Acme.Workforce.Workplace.Booking.BookingVariance;

/// <summary>Checks of booking variance code, run by the project's runner.</summary>
public static class WeightedBookingVarianceChecks
{
    public static void WeightedBookingVarianceStartsInRequested()
    {
        var item = WeightedBookingVariance.NewWeightedBookingVariance("code-753", "name-226");
        Check.Equal(item.Status, WeightedBookingVarianceStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedBookingVarianceMovesToRevoked()
    {
        var item = WeightedBookingVariance.NewWeightedBookingVariance("code-753", "name-226");
        try
        {
            item.MoveTo(WeightedBookingVarianceStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedBookingVarianceStatus.Revoked, "status");
    }

    public static void WeightedBookingVarianceRejectsMovesFromExpired()
    {
        var item = WeightedBookingVariance.NewWeightedBookingVariance("code-753", "name-226");
        item.Status = WeightedBookingVarianceStatus.Expired;
        Check.Fails(() => item.MoveTo(WeightedBookingVarianceStatus.Requested), "move");
    }

    public static void RunAll()
    {
        WeightedBookingVarianceStartsInRequested();
        WeightedBookingVarianceMovesToRevoked();
        WeightedBookingVarianceRejectsMovesFromExpired();
    }
}
