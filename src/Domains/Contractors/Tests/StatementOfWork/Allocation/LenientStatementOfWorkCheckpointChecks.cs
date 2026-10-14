namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Allocation;

using Acme.Workforce.Contractors.StatementOfWork.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class LenientStatementOfWorkCheckpointChecks
{
    public static void LenientStatementOfWorkCheckpointStartsInDraft()
    {
        var item = LenientStatementOfWorkCheckpoint.NewLenientStatementOfWorkCheckpoint("code-495", "name-934");
        Check.Equal(item.Status, LenientStatementOfWorkCheckpointStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientStatementOfWorkCheckpointMovesToActive()
    {
        var item = LenientStatementOfWorkCheckpoint.NewLenientStatementOfWorkCheckpoint("code-495", "name-934");
        try
        {
            item.MoveTo(LenientStatementOfWorkCheckpointStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientStatementOfWorkCheckpointStatus.Active, "status");
    }

    public static void LenientStatementOfWorkCheckpointRejectsMovesFromClosed()
    {
        var item = LenientStatementOfWorkCheckpoint.NewLenientStatementOfWorkCheckpoint("code-495", "name-934");
        item.Status = LenientStatementOfWorkCheckpointStatus.Closed;
        Check.Fails(() => item.MoveTo(LenientStatementOfWorkCheckpointStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LenientStatementOfWorkCheckpointStartsInDraft();
        LenientStatementOfWorkCheckpointMovesToActive();
        LenientStatementOfWorkCheckpointRejectsMovesFromClosed();
    }
}
