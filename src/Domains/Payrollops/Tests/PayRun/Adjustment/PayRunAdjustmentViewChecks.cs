namespace Acme.Workforce.Payrollops.Tests.PayRun.Adjustment;

using Acme.Workforce.Payrollops.PayRun.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class PayRunAdjustmentViewChecks
{
    public static void PayRunAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = PayRunAdjustment.NewPayRunAdjustment("code-746", "name-841");
        var view = PayRunAdjustmentView.ToPayRunAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayRunAdjustmentView.ToPayRunAdjustmentViews(new List<PayRunAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayRunAdjustmentViewShowsTheStatusInCapitals();
    }
}
