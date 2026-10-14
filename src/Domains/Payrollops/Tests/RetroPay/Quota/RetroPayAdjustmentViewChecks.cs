namespace Acme.Workforce.Payrollops.Tests.RetroPay.Quota;

using Acme.Workforce.Payrollops.RetroPay.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class RetroPayAdjustmentViewChecks
{
    public static void RetroPayAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = RetroPayAdjustment.NewRetroPayAdjustment("code-775", "label-65");
        var view = RetroPayAdjustmentView.ToRetroPayAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RetroPayAdjustmentView.ToRetroPayAdjustmentViews(new List<RetroPayAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RetroPayAdjustmentViewShowsTheStatusInCapitals();
    }
}
