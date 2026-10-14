namespace Acme.Workforce.Payrollops.Tests.Payslip.Slot;

using Acme.Workforce.Payrollops.Payslip.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class PayslipSlotViewChecks
{
    public static void PayslipSlotViewShowsTheStatusInCapitals()
    {
        var item = PayslipSlot.NewPayslipSlot("code-750", "title-118");
        var view = PayslipSlotView.ToPayslipSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayslipSlotView.ToPayslipSlotViews(new List<PayslipSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayslipSlotViewShowsTheStatusInCapitals();
    }
}
