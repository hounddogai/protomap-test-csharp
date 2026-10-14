namespace Acme.Workforce.Compliance.Tests.Exception.Allocation;

using Acme.Workforce.Compliance.Exception.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class ExceptionSlotChecks
{
    public static void ExceptionSlotStartsInProposed()
    {
        var item = ExceptionSlot.NewExceptionSlot("code-715", "name-79");
        Check.Equal(item.Status, ExceptionSlotStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ExceptionSlotMovesToRejected()
    {
        var item = ExceptionSlot.NewExceptionSlot("code-715", "name-79");
        try
        {
            item.MoveTo(ExceptionSlotStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ExceptionSlotStatus.Rejected, "status");
    }

    public static void ExceptionSlotRejectsMovesFromWithdrawn()
    {
        var item = ExceptionSlot.NewExceptionSlot("code-715", "name-79");
        item.Status = ExceptionSlotStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ExceptionSlotStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ExceptionSlotStartsInProposed();
        ExceptionSlotMovesToRejected();
        ExceptionSlotRejectsMovesFromWithdrawn();
    }
}
