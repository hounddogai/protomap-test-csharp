namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyAllocation;

using Acme.Workforce.Assets.Warranty.WarrantyAllocation;

/// <summary>Checks of warranty allocation code, run by the project's runner.</summary>
public static class PendingWarrantyAllocationViewChecks
{
    public static void PendingWarrantyAllocationViewShowsTheStatusInCapitals()
    {
        var item = PendingWarrantyAllocation.NewPendingWarrantyAllocation("code-596", "title-281");
        var view = PendingWarrantyAllocationView.ToPendingWarrantyAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingWarrantyAllocationView.ToPendingWarrantyAllocationViews(new List<PendingWarrantyAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingWarrantyAllocationViewShowsTheStatusInCapitals();
    }
}
