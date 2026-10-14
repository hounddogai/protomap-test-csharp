namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Snapshot;

using Acme.Workforce.Contractors.StatementOfWork.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class DefaultStatementOfWorkMappingChecks
{
    public static void DefaultStatementOfWorkMappingStartsInDraft()
    {
        var item = DefaultStatementOfWorkMapping.NewDefaultStatementOfWorkMapping("code-733", "label-326");
        Check.Equal(item.Status, DefaultStatementOfWorkMappingStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultStatementOfWorkMappingMovesToSuspended()
    {
        var item = DefaultStatementOfWorkMapping.NewDefaultStatementOfWorkMapping("code-733", "label-326");
        try
        {
            item.MoveTo(DefaultStatementOfWorkMappingStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultStatementOfWorkMappingStatus.Suspended, "status");
    }

    public static void DefaultStatementOfWorkMappingRejectsMovesFromClosed()
    {
        var item = DefaultStatementOfWorkMapping.NewDefaultStatementOfWorkMapping("code-733", "label-326");
        item.Status = DefaultStatementOfWorkMappingStatus.Closed;
        Check.Fails(() => item.MoveTo(DefaultStatementOfWorkMappingStatus.Draft), "move");
    }

    public static void RunAll()
    {
        DefaultStatementOfWorkMappingStartsInDraft();
        DefaultStatementOfWorkMappingMovesToSuspended();
        DefaultStatementOfWorkMappingRejectsMovesFromClosed();
    }
}
