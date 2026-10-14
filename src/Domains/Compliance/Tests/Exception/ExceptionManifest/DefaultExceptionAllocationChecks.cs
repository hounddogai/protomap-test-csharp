namespace Acme.Workforce.Compliance.Tests.Exception.ExceptionManifest;

using Acme.Workforce.Compliance.Exception.ExceptionManifest;

/// <summary>Checks of exception manifest code, run by the project's runner.</summary>
public static class DefaultExceptionAllocationChecks
{
    public static void DefaultExceptionAllocationStartsInPlanned()
    {
        var item = DefaultExceptionAllocation.NewDefaultExceptionAllocation("code-341", "name-148");
        Check.Equal(item.Status, DefaultExceptionAllocationStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultExceptionAllocationMovesToCancelled()
    {
        var item = DefaultExceptionAllocation.NewDefaultExceptionAllocation("code-341", "name-148");
        try
        {
            item.MoveTo(DefaultExceptionAllocationStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultExceptionAllocationStatus.Cancelled, "status");
    }

    public static void DefaultExceptionAllocationRejectsMovesFromCancelled()
    {
        var item = DefaultExceptionAllocation.NewDefaultExceptionAllocation("code-341", "name-148");
        item.Status = DefaultExceptionAllocationStatus.Cancelled;
        Check.Fails(() => item.MoveTo(DefaultExceptionAllocationStatus.Planned), "move");
    }

    public static void RunAll()
    {
        DefaultExceptionAllocationStartsInPlanned();
        DefaultExceptionAllocationMovesToCancelled();
        DefaultExceptionAllocationRejectsMovesFromCancelled();
    }
}
