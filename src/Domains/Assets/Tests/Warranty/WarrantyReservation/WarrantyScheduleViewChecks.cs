namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyReservation;

using Acme.Workforce.Assets.Warranty.WarrantyReservation;

/// <summary>Checks of warranty reservation code, run by the project's runner.</summary>
public static class WarrantyScheduleViewChecks
{
    public static void WarrantyScheduleViewShowsTheStatusInCapitals()
    {
        var item = WarrantySchedule.NewWarrantySchedule("code-377", "label-751");
        var view = WarrantyScheduleView.ToWarrantyScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WarrantyScheduleView.ToWarrantyScheduleViews(new List<WarrantySchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WarrantyScheduleViewShowsTheStatusInCapitals();
    }
}
