namespace Acme.Workforce.Payrollops.Tests.PayRun.Ledger;

using Acme.Workforce.Payrollops.PayRun.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class PayRunMappingViewChecks
{
    public static void PayRunMappingViewShowsTheStatusInCapitals()
    {
        var item = PayRunMapping.NewPayRunMapping("code-153", "name-757");
        var view = PayRunMappingView.ToPayRunMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayRunMappingView.ToPayRunMappingViews(new List<PayRunMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayRunMappingViewShowsTheStatusInCapitals();
    }
}
