namespace Acme.Workforce.Payrollops.Tests.Payslip.Allocation;

using Acme.Workforce.Payrollops.Payslip.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class ManualPayslipAllocationViewChecks
{
    public static void ManualPayslipAllocationViewShowsTheStatusInCapitals()
    {
        var item = ManualPayslipAllocation.NewManualPayslipAllocation("code-756", "name-132");
        var view = ManualPayslipAllocationView.ToManualPayslipAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualPayslipAllocationView.ToManualPayslipAllocationViews(new List<ManualPayslipAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualPayslipAllocationViewShowsTheStatusInCapitals();
    }
}
