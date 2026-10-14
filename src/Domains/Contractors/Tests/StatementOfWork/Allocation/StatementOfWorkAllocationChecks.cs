namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Allocation;

using Acme.Workforce.Contractors.StatementOfWork.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class StatementOfWorkAllocationChecks
{
    public static void StatementOfWorkAllocationStartsInDraft()
    {
        var item = StatementOfWorkAllocation.NewStatementOfWorkAllocation("code-539", "name-955");
        Check.Equal(item.Status, StatementOfWorkAllocationStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StatementOfWorkAllocationMovesToPending()
    {
        var item = StatementOfWorkAllocation.NewStatementOfWorkAllocation("code-539", "name-955");
        try
        {
            item.MoveTo(StatementOfWorkAllocationStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StatementOfWorkAllocationStatus.Pending, "status");
    }

    public static void StatementOfWorkAllocationRejectsMovesFromClosed()
    {
        var item = StatementOfWorkAllocation.NewStatementOfWorkAllocation("code-539", "name-955");
        item.Status = StatementOfWorkAllocationStatus.Closed;
        Check.Fails(() => item.MoveTo(StatementOfWorkAllocationStatus.Draft), "move");
    }

    public static void RunAll()
    {
        StatementOfWorkAllocationStartsInDraft();
        StatementOfWorkAllocationMovesToPending();
        StatementOfWorkAllocationRejectsMovesFromClosed();
    }
}
