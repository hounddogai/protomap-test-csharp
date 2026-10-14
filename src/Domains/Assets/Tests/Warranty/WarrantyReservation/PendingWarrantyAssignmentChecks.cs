namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyReservation;

using Acme.Workforce.Assets.Warranty.WarrantyReservation;

/// <summary>Checks of warranty reservation code, run by the project's runner.</summary>
public static class PendingWarrantyAssignmentChecks
{
    public static void PendingWarrantyAssignmentStartsInCreated()
    {
        var item = PendingWarrantyAssignment.NewPendingWarrantyAssignment("code-543", "title-327");
        Check.Equal(item.Status, PendingWarrantyAssignmentStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingWarrantyAssignmentMovesToPublished()
    {
        var item = PendingWarrantyAssignment.NewPendingWarrantyAssignment("code-543", "title-327");
        try
        {
            item.MoveTo(PendingWarrantyAssignmentStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingWarrantyAssignmentStatus.Published, "status");
    }

    public static void PendingWarrantyAssignmentRejectsMovesFromRetired()
    {
        var item = PendingWarrantyAssignment.NewPendingWarrantyAssignment("code-543", "title-327");
        item.Status = PendingWarrantyAssignmentStatus.Retired;
        Check.Fails(() => item.MoveTo(PendingWarrantyAssignmentStatus.Created), "move");
    }

    public static void RunAll()
    {
        PendingWarrantyAssignmentStartsInCreated();
        PendingWarrantyAssignmentMovesToPublished();
        PendingWarrantyAssignmentRejectsMovesFromRetired();
    }
}
