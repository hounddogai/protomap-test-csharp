namespace Acme.Workforce.Payrollops.Tests.Payslip.Manifest;

using Acme.Workforce.Payrollops.Payslip.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class PayslipEntryViewChecks
{
    public static void PayslipEntryViewShowsTheStatusInCapitals()
    {
        var item = PayslipEntry.NewPayslipEntry("code-723", "title-119");
        var view = PayslipEntryView.ToPayslipEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayslipEntryView.ToPayslipEntryViews(new List<PayslipEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayslipEntryViewShowsTheStatusInCapitals();
    }
}
