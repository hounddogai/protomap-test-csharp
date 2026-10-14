namespace Acme.Workforce.Compensation.Tests.Adjustment.Allocation;

using Acme.Workforce.Compensation.Adjustment.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class AdjustmentAllocationViewChecks
{
    public static void AdjustmentAllocationViewShowsTheStatusInCapitals()
    {
        var item = AdjustmentAllocation.NewAdjustmentAllocation("code-181", "name-764");
        var view = AdjustmentAllocationView.ToAdjustmentAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdjustmentAllocationView.ToAdjustmentAllocationViews(new List<AdjustmentAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdjustmentAllocationViewShowsTheStatusInCapitals();
    }
}
