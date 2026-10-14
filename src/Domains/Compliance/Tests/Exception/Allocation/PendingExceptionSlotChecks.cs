namespace Acme.Workforce.Compliance.Tests.Exception.Allocation;

using Acme.Workforce.Compliance.Exception.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class PendingExceptionSlotChecks
{
    public static void PendingExceptionSlotStartsInProposed()
    {
        var item = PendingExceptionSlot.NewPendingExceptionSlot("code-412", "title-245");
        Check.Equal(item.Status, PendingExceptionSlotStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingExceptionSlotMovesToApproved()
    {
        var item = PendingExceptionSlot.NewPendingExceptionSlot("code-412", "title-245");
        try
        {
            item.MoveTo(PendingExceptionSlotStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingExceptionSlotStatus.Approved, "status");
    }

    public static void PendingExceptionSlotRejectsMovesFromWithdrawn()
    {
        var item = PendingExceptionSlot.NewPendingExceptionSlot("code-412", "title-245");
        item.Status = PendingExceptionSlotStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PendingExceptionSlotStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PendingExceptionSlotStartsInProposed();
        PendingExceptionSlotMovesToApproved();
        PendingExceptionSlotRejectsMovesFromWithdrawn();
    }
}
