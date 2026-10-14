namespace Acme.Workforce.Talent.Tests.Pool.Budget;

using Acme.Workforce.Talent.Pool.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class PoolSlotViewChecks
{
    public static void PoolSlotViewShowsTheStatusInCapitals()
    {
        var item = PoolSlot.NewPoolSlot("code-664", "label-868");
        var view = PoolSlotView.ToPoolSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PoolSlotView.ToPoolSlotViews(new List<PoolSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PoolSlotViewShowsTheStatusInCapitals();
    }
}
