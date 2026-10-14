namespace Acme.Workforce.Contractors.Tests.Timesheet.TimesheetPolicy;

using Acme.Workforce.Contractors.Timesheet.TimesheetPolicy;

/// <summary>Checks of timesheet policy code, run by the project's runner.</summary>
public static class PendingTimesheetPolicyViewChecks
{
    public static void PendingTimesheetPolicyViewShowsTheStatusInCapitals()
    {
        var item = PendingTimesheetPolicy.NewPendingTimesheetPolicy("code-350", "title-741");
        var view = PendingTimesheetPolicyView.ToPendingTimesheetPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingTimesheetPolicyView.ToPendingTimesheetPolicyViews(new List<PendingTimesheetPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingTimesheetPolicyViewShowsTheStatusInCapitals();
    }
}
