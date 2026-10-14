namespace Acme.Workforce.Assets.Tests.LicenseSeat.Assignment;

using Acme.Workforce.Assets.LicenseSeat.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class GlobalLicenseSeatAssignmentViewChecks
{
    public static void GlobalLicenseSeatAssignmentViewShowsTheStatusInCapitals()
    {
        var item = GlobalLicenseSeatAssignment.NewGlobalLicenseSeatAssignment("code-743", "name-27");
        var view = GlobalLicenseSeatAssignmentView.ToGlobalLicenseSeatAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalLicenseSeatAssignmentView.ToGlobalLicenseSeatAssignmentViews(new List<GlobalLicenseSeatAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalLicenseSeatAssignmentViewShowsTheStatusInCapitals();
    }
}
