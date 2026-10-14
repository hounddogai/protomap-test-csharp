namespace Acme.Workforce.Workplace.Tests.Booking.Group;

using Acme.Workforce.Workplace.Booking.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class CappedBookingTargetViewChecks
{
    public static void CappedBookingTargetViewShowsTheStatusInCapitals()
    {
        var item = CappedBookingTarget.NewCappedBookingTarget("code-368", "name-196");
        var view = CappedBookingTargetView.ToCappedBookingTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedBookingTargetView.ToCappedBookingTargetViews(new List<CappedBookingTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedBookingTargetViewShowsTheStatusInCapitals();
    }
}
