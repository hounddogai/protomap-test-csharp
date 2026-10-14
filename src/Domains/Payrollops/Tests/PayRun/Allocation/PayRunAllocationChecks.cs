namespace Acme.Workforce.Payrollops.Tests.PayRun.Allocation;

using Acme.Workforce.Payrollops.PayRun.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class PayRunAllocationChecks
{
    public static void PayRunAllocationStartsInCreated()
    {
        var item = PayRunAllocation.NewPayRunAllocation("code-815", "label-154");
        Check.Equal(item.Status, PayRunAllocationStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayRunAllocationMovesToValidated()
    {
        var item = PayRunAllocation.NewPayRunAllocation("code-815", "label-154");
        try
        {
            item.MoveTo(PayRunAllocationStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayRunAllocationStatus.Validated, "status");
    }

    public static void PayRunAllocationRejectsMovesFromRetired()
    {
        var item = PayRunAllocation.NewPayRunAllocation("code-815", "label-154");
        item.Status = PayRunAllocationStatus.Retired;
        Check.Fails(() => item.MoveTo(PayRunAllocationStatus.Created), "move");
    }

    public static void RunAll()
    {
        PayRunAllocationStartsInCreated();
        PayRunAllocationMovesToValidated();
        PayRunAllocationRejectsMovesFromRetired();
    }
}
