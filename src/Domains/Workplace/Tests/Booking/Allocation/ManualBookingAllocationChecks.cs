namespace Acme.Workforce.Workplace.Tests.Booking.Allocation;

using Acme.Workforce.Workplace.Booking.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class ManualBookingAllocationChecks
{
    public static void ManualBookingAllocationStartsInProposed()
    {
        var item = ManualBookingAllocation.NewManualBookingAllocation("code-753", "label-243");
        Check.Equal(item.Status, ManualBookingAllocationStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualBookingAllocationMovesToRejected()
    {
        var item = ManualBookingAllocation.NewManualBookingAllocation("code-753", "label-243");
        try
        {
            item.MoveTo(ManualBookingAllocationStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualBookingAllocationStatus.Rejected, "status");
    }

    public static void ManualBookingAllocationRejectsMovesFromWithdrawn()
    {
        var item = ManualBookingAllocation.NewManualBookingAllocation("code-753", "label-243");
        item.Status = ManualBookingAllocationStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ManualBookingAllocationStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ManualBookingAllocationStartsInProposed();
        ManualBookingAllocationMovesToRejected();
        ManualBookingAllocationRejectsMovesFromWithdrawn();
    }
}
