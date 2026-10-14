namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatMilestone;

using Acme.Workforce.Assets.LicenseSeat.SeatMilestone;

/// <summary>Checks of seat milestone code, run by the project's runner.</summary>
public static class LicenseSeatAssignmentViewChecks
{
    public static void LicenseSeatAssignmentViewShowsTheStatusInCapitals()
    {
        var item = LicenseSeatAssignment.NewLicenseSeatAssignment("code-889", "label-143");
        var view = LicenseSeatAssignmentView.ToLicenseSeatAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LicenseSeatAssignmentView.ToLicenseSeatAssignmentViews(new List<LicenseSeatAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LicenseSeatAssignmentViewShowsTheStatusInCapitals();
    }
}
