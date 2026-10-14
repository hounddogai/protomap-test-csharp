namespace Acme.Workforce.Absence.Tests.LeaveRequest.Adjustment;

using Acme.Workforce.Absence.LeaveRequest.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LeaveRequestAdjustmentViewChecks
{
    public static void LeaveRequestAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = LeaveRequestAdjustment.NewLeaveRequestAdjustment("code-117", "title-792");
        var view = LeaveRequestAdjustmentView.ToLeaveRequestAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LeaveRequestAdjustmentView.ToLeaveRequestAdjustmentViews(new List<LeaveRequestAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LeaveRequestAdjustmentViewShowsTheStatusInCapitals();
    }
}
