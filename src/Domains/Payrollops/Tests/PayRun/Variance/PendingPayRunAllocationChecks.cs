namespace Acme.Workforce.Payrollops.Tests.PayRun.Variance;

using Acme.Workforce.Payrollops.PayRun.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class PendingPayRunAllocationChecks
{
    public static void PendingPayRunAllocationStartsInIdle()
    {
        var item = PendingPayRunAllocation.NewPendingPayRunAllocation("code-908", "name-89");
        Check.Equal(item.Status, PendingPayRunAllocationStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingPayRunAllocationMovesToWarming()
    {
        var item = PendingPayRunAllocation.NewPendingPayRunAllocation("code-908", "name-89");
        try
        {
            item.MoveTo(PendingPayRunAllocationStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingPayRunAllocationStatus.Warming, "status");
    }

    public static void PendingPayRunAllocationRejectsMovesFromStopped()
    {
        var item = PendingPayRunAllocation.NewPendingPayRunAllocation("code-908", "name-89");
        item.Status = PendingPayRunAllocationStatus.Stopped;
        Check.Fails(() => item.MoveTo(PendingPayRunAllocationStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PendingPayRunAllocationStartsInIdle();
        PendingPayRunAllocationMovesToWarming();
        PendingPayRunAllocationRejectsMovesFromStopped();
    }
}
