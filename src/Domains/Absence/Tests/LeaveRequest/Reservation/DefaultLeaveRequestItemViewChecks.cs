namespace Acme.Workforce.Absence.Tests.LeaveRequest.Reservation;

using Acme.Workforce.Absence.LeaveRequest.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class DefaultLeaveRequestItemViewChecks
{
    public static void DefaultLeaveRequestItemViewShowsTheStatusInCapitals()
    {
        var item = DefaultLeaveRequestItem.NewDefaultLeaveRequestItem("code-401", "title-451");
        var view = DefaultLeaveRequestItemView.ToDefaultLeaveRequestItemView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultLeaveRequestItemView.ToDefaultLeaveRequestItemViews(new List<DefaultLeaveRequestItem> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultLeaveRequestItemViewShowsTheStatusInCapitals();
    }
}
