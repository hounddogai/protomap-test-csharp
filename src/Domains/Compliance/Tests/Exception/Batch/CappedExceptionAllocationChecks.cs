namespace Acme.Workforce.Compliance.Tests.Exception.Batch;

using Acme.Workforce.Compliance.Exception.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class CappedExceptionAllocationChecks
{
    public static void CappedExceptionAllocationStartsInOpen()
    {
        var item = CappedExceptionAllocation.NewCappedExceptionAllocation("code-409", "label-55");
        Check.Equal(item.Status, CappedExceptionAllocationStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedExceptionAllocationMovesToResolved()
    {
        var item = CappedExceptionAllocation.NewCappedExceptionAllocation("code-409", "label-55");
        try
        {
            item.MoveTo(CappedExceptionAllocationStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedExceptionAllocationStatus.Resolved, "status");
    }

    public static void CappedExceptionAllocationRejectsMovesFromArchived()
    {
        var item = CappedExceptionAllocation.NewCappedExceptionAllocation("code-409", "label-55");
        item.Status = CappedExceptionAllocationStatus.Archived;
        Check.Fails(() => item.MoveTo(CappedExceptionAllocationStatus.Open), "move");
    }

    public static void RunAll()
    {
        CappedExceptionAllocationStartsInOpen();
        CappedExceptionAllocationMovesToResolved();
        CappedExceptionAllocationRejectsMovesFromArchived();
    }
}
