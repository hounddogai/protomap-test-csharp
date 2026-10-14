namespace Acme.Workforce.Payrollops.Tests.RetroPay.Allocation;

using Acme.Workforce.Payrollops.RetroPay.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class DefaultRetroPayAllocationChecks
{
    public static void DefaultRetroPayAllocationStartsInProposed()
    {
        var item = DefaultRetroPayAllocation.NewDefaultRetroPayAllocation("code-568", "title-277");
        Check.Equal(item.Status, DefaultRetroPayAllocationStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultRetroPayAllocationMovesToWithdrawn()
    {
        var item = DefaultRetroPayAllocation.NewDefaultRetroPayAllocation("code-568", "title-277");
        try
        {
            item.MoveTo(DefaultRetroPayAllocationStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultRetroPayAllocationStatus.Withdrawn, "status");
    }

    public static void DefaultRetroPayAllocationRejectsMovesFromWithdrawn()
    {
        var item = DefaultRetroPayAllocation.NewDefaultRetroPayAllocation("code-568", "title-277");
        item.Status = DefaultRetroPayAllocationStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(DefaultRetroPayAllocationStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        DefaultRetroPayAllocationStartsInProposed();
        DefaultRetroPayAllocationMovesToWithdrawn();
        DefaultRetroPayAllocationRejectsMovesFromWithdrawn();
    }
}
