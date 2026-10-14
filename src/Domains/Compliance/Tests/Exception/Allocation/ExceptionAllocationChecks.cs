namespace Acme.Workforce.Compliance.Tests.Exception.Allocation;

using Acme.Workforce.Compliance.Exception.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class ExceptionAllocationChecks
{
    public static void ExceptionAllocationStartsInIdle()
    {
        var item = ExceptionAllocation.NewExceptionAllocation("code-622", "name-919");
        Check.Equal(item.Status, ExceptionAllocationStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ExceptionAllocationMovesToWarming()
    {
        var item = ExceptionAllocation.NewExceptionAllocation("code-622", "name-919");
        try
        {
            item.MoveTo(ExceptionAllocationStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ExceptionAllocationStatus.Warming, "status");
    }

    public static void ExceptionAllocationRejectsMovesFromStopped()
    {
        var item = ExceptionAllocation.NewExceptionAllocation("code-622", "name-919");
        item.Status = ExceptionAllocationStatus.Stopped;
        Check.Fails(() => item.MoveTo(ExceptionAllocationStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ExceptionAllocationStartsInIdle();
        ExceptionAllocationMovesToWarming();
        ExceptionAllocationRejectsMovesFromStopped();
    }
}
