namespace Acme.Workforce.Payrollops.Tests.PayRun.RunAllocation;

using Acme.Workforce.Payrollops.PayRun.RunAllocation;

/// <summary>Checks of run allocation code, run by the project's runner.</summary>
public static class AdaptivePayRunAllocationViewChecks
{
    public static void AdaptivePayRunAllocationViewShowsTheStatusInCapitals()
    {
        var item = AdaptivePayRunAllocation.NewAdaptivePayRunAllocation("code-524", "title-322");
        var view = AdaptivePayRunAllocationView.ToAdaptivePayRunAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptivePayRunAllocationView.ToAdaptivePayRunAllocationViews(new List<AdaptivePayRunAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptivePayRunAllocationViewShowsTheStatusInCapitals();
    }
}
