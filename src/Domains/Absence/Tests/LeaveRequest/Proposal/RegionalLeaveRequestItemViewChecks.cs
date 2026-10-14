namespace Acme.Workforce.Absence.Tests.LeaveRequest.Proposal;

using Acme.Workforce.Absence.LeaveRequest.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class RegionalLeaveRequestItemViewChecks
{
    public static void RegionalLeaveRequestItemViewShowsTheStatusInCapitals()
    {
        var item = RegionalLeaveRequestItem.NewRegionalLeaveRequestItem("code-739", "label-952");
        var view = RegionalLeaveRequestItemView.ToRegionalLeaveRequestItemView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalLeaveRequestItemView.ToRegionalLeaveRequestItemViews(new List<RegionalLeaveRequestItem> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalLeaveRequestItemViewShowsTheStatusInCapitals();
    }
}
