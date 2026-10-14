namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyReservation;

using Acme.Workforce.Assets.Warranty.WarrantyReservation;

/// <summary>Checks of warranty reservation code, run by the project's runner.</summary>
public static class PendingWarrantyAssignmentViewChecks
{
    public static void PendingWarrantyAssignmentViewShowsTheStatusInCapitals()
    {
        var item = PendingWarrantyAssignment.NewPendingWarrantyAssignment("code-543", "title-327");
        var view = PendingWarrantyAssignmentView.ToPendingWarrantyAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingWarrantyAssignmentView.ToPendingWarrantyAssignmentViews(new List<PendingWarrantyAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingWarrantyAssignmentViewShowsTheStatusInCapitals();
    }
}
