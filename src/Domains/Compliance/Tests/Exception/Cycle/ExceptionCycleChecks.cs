namespace Acme.Workforce.Compliance.Tests.Exception.Cycle;

using Acme.Workforce.Compliance.Exception.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class ExceptionCycleChecks
{
    public static void ExceptionCycleStartsInProposed()
    {
        var item = ExceptionCycle.NewExceptionCycle("code-540", "label-489");
        Check.Equal(item.Status, ExceptionCycleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ExceptionCycleMovesToApproved()
    {
        var item = ExceptionCycle.NewExceptionCycle("code-540", "label-489");
        try
        {
            item.MoveTo(ExceptionCycleStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ExceptionCycleStatus.Approved, "status");
    }

    public static void ExceptionCycleRejectsMovesFromWithdrawn()
    {
        var item = ExceptionCycle.NewExceptionCycle("code-540", "label-489");
        item.Status = ExceptionCycleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ExceptionCycleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ExceptionCycleStartsInProposed();
        ExceptionCycleMovesToApproved();
        ExceptionCycleRejectsMovesFromWithdrawn();
    }
}
