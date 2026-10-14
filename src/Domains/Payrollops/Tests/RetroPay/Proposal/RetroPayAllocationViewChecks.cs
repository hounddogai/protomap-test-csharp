namespace Acme.Workforce.Payrollops.Tests.RetroPay.Proposal;

using Acme.Workforce.Payrollops.RetroPay.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class RetroPayAllocationViewChecks
{
    public static void RetroPayAllocationViewShowsTheStatusInCapitals()
    {
        var item = RetroPayAllocation.NewRetroPayAllocation("code-399", "label-484");
        var view = RetroPayAllocationView.ToRetroPayAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RetroPayAllocationView.ToRetroPayAllocationViews(new List<RetroPayAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RetroPayAllocationViewShowsTheStatusInCapitals();
    }
}
