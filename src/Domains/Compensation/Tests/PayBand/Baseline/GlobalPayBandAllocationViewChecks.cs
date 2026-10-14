namespace Acme.Workforce.Compensation.Tests.PayBand.Baseline;

using Acme.Workforce.Compensation.PayBand.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class GlobalPayBandAllocationViewChecks
{
    public static void GlobalPayBandAllocationViewShowsTheStatusInCapitals()
    {
        var item = GlobalPayBandAllocation.NewGlobalPayBandAllocation("code-682", "title-244");
        var view = GlobalPayBandAllocationView.ToGlobalPayBandAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalPayBandAllocationView.ToGlobalPayBandAllocationViews(new List<GlobalPayBandAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalPayBandAllocationViewShowsTheStatusInCapitals();
    }
}
