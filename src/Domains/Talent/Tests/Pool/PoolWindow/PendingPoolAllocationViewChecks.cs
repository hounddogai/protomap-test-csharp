namespace Acme.Workforce.Talent.Tests.Pool.PoolWindow;

using Acme.Workforce.Talent.Pool.PoolWindow;

/// <summary>Checks of pool window code, run by the project's runner.</summary>
public static class PendingPoolAllocationViewChecks
{
    public static void PendingPoolAllocationViewShowsTheStatusInCapitals()
    {
        var item = PendingPoolAllocation.NewPendingPoolAllocation("code-485", "title-634");
        var view = PendingPoolAllocationView.ToPendingPoolAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingPoolAllocationView.ToPendingPoolAllocationViews(new List<PendingPoolAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingPoolAllocationViewShowsTheStatusInCapitals();
    }
}
