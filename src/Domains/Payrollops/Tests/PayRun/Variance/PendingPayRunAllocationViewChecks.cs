namespace Acme.Workforce.Payrollops.Tests.PayRun.Variance;

using Acme.Workforce.Payrollops.PayRun.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class PendingPayRunAllocationViewChecks
{
    public static void PendingPayRunAllocationViewShowsTheStatusInCapitals()
    {
        var item = PendingPayRunAllocation.NewPendingPayRunAllocation("code-908", "name-89");
        var view = PendingPayRunAllocationView.ToPendingPayRunAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingPayRunAllocationView.ToPendingPayRunAllocationViews(new List<PendingPayRunAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingPayRunAllocationViewShowsTheStatusInCapitals();
    }
}
