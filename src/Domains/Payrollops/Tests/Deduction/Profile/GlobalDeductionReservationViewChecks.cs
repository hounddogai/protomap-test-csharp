namespace Acme.Workforce.Payrollops.Tests.Deduction.Profile;

using Acme.Workforce.Payrollops.Deduction.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class GlobalDeductionReservationViewChecks
{
    public static void GlobalDeductionReservationViewShowsTheStatusInCapitals()
    {
        var item = GlobalDeductionReservation.NewGlobalDeductionReservation("code-479", "title-418");
        var view = GlobalDeductionReservationView.ToGlobalDeductionReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalDeductionReservationView.ToGlobalDeductionReservationViews(new List<GlobalDeductionReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalDeductionReservationViewShowsTheStatusInCapitals();
    }
}
