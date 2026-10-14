namespace Acme.Workforce.Payrollops.Tests.Deduction.Budget;

using Acme.Workforce.Payrollops.Deduction.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class DeductionReservationViewChecks
{
    public static void DeductionReservationViewShowsTheStatusInCapitals()
    {
        var item = DeductionReservation.NewDeductionReservation("code-676", "name-63");
        var view = DeductionReservationView.ToDeductionReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DeductionReservationView.ToDeductionReservationViews(new List<DeductionReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DeductionReservationViewShowsTheStatusInCapitals();
    }
}
