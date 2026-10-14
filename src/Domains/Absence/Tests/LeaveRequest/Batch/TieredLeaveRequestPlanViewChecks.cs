namespace Acme.Workforce.Absence.Tests.LeaveRequest.Batch;

using Acme.Workforce.Absence.LeaveRequest.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class TieredLeaveRequestPlanViewChecks
{
    public static void TieredLeaveRequestPlanViewShowsTheStatusInCapitals()
    {
        var item = TieredLeaveRequestPlan.NewTieredLeaveRequestPlan("code-105", "label-718");
        var view = TieredLeaveRequestPlanView.ToTieredLeaveRequestPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredLeaveRequestPlanView.ToTieredLeaveRequestPlanViews(new List<TieredLeaveRequestPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredLeaveRequestPlanViewShowsTheStatusInCapitals();
    }
}
