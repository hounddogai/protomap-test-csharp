namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Baseline;

using Acme.Workforce.Contractors.StatementOfWork.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PrimaryStatementOfWorkAllocationChecks
{
    public static void PrimaryStatementOfWorkAllocationStartsInIdle()
    {
        var item = PrimaryStatementOfWorkAllocation.NewPrimaryStatementOfWorkAllocation("code-52", "label-407");
        Check.Equal(item.Status, PrimaryStatementOfWorkAllocationStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryStatementOfWorkAllocationMovesToReady()
    {
        var item = PrimaryStatementOfWorkAllocation.NewPrimaryStatementOfWorkAllocation("code-52", "label-407");
        try
        {
            item.MoveTo(PrimaryStatementOfWorkAllocationStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryStatementOfWorkAllocationStatus.Ready, "status");
    }

    public static void PrimaryStatementOfWorkAllocationRejectsMovesFromStopped()
    {
        var item = PrimaryStatementOfWorkAllocation.NewPrimaryStatementOfWorkAllocation("code-52", "label-407");
        item.Status = PrimaryStatementOfWorkAllocationStatus.Stopped;
        Check.Fails(() => item.MoveTo(PrimaryStatementOfWorkAllocationStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PrimaryStatementOfWorkAllocationStartsInIdle();
        PrimaryStatementOfWorkAllocationMovesToReady();
        PrimaryStatementOfWorkAllocationRejectsMovesFromStopped();
    }
}
