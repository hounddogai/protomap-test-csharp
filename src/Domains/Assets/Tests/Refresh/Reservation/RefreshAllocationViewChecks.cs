namespace Acme.Workforce.Assets.Tests.Refresh.Reservation;

using Acme.Workforce.Assets.Refresh.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class RefreshAllocationViewChecks
{
    public static void RefreshAllocationViewShowsTheStatusInCapitals()
    {
        var item = RefreshAllocation.NewRefreshAllocation("code-106", "name-417");
        var view = RefreshAllocationView.ToRefreshAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RefreshAllocationView.ToRefreshAllocationViews(new List<RefreshAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RefreshAllocationViewShowsTheStatusInCapitals();
    }
}
