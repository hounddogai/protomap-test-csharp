namespace Acme.Workforce.Payrollops.Tests.Payslip.Manifest;

using Acme.Workforce.Payrollops.Payslip.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class RegionalPayslipReservationViewChecks
{
    public static void RegionalPayslipReservationViewShowsTheStatusInCapitals()
    {
        var item = RegionalPayslipReservation.NewRegionalPayslipReservation("code-809", "label-200");
        var view = RegionalPayslipReservationView.ToRegionalPayslipReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalPayslipReservationView.ToRegionalPayslipReservationViews(new List<RegionalPayslipReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalPayslipReservationViewShowsTheStatusInCapitals();
    }
}
