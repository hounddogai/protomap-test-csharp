namespace Acme.Workforce.Payrollops.Tests.RetroPay.Proposal;

using Acme.Workforce.Payrollops.RetroPay.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class RetroPayAllocationChecks
{
    public static void RetroPayAllocationStartsInRequested()
    {
        var item = RetroPayAllocation.NewRetroPayAllocation("code-399", "label-484");
        Check.Equal(item.Status, RetroPayAllocationStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RetroPayAllocationMovesToGranted()
    {
        var item = RetroPayAllocation.NewRetroPayAllocation("code-399", "label-484");
        try
        {
            item.MoveTo(RetroPayAllocationStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RetroPayAllocationStatus.Granted, "status");
    }

    public static void RetroPayAllocationRejectsMovesFromExpired()
    {
        var item = RetroPayAllocation.NewRetroPayAllocation("code-399", "label-484");
        item.Status = RetroPayAllocationStatus.Expired;
        Check.Fails(() => item.MoveTo(RetroPayAllocationStatus.Requested), "move");
    }

    public static void RunAll()
    {
        RetroPayAllocationStartsInRequested();
        RetroPayAllocationMovesToGranted();
        RetroPayAllocationRejectsMovesFromExpired();
    }
}
