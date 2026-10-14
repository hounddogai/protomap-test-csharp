namespace Acme.Workforce.Absence.Tests.LeaveRequest.Item;

using Acme.Workforce.Absence.LeaveRequest.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class LeaveRequestItemViewChecks
{
    public static void LeaveRequestItemViewShowsTheStatusInCapitals()
    {
        var item = LeaveRequestItem.NewLeaveRequestItem("code-716", "title-327");
        var view = LeaveRequestItemView.ToLeaveRequestItemView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LeaveRequestItemView.ToLeaveRequestItemViews(new List<LeaveRequestItem> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LeaveRequestItemViewShowsTheStatusInCapitals();
    }
}
