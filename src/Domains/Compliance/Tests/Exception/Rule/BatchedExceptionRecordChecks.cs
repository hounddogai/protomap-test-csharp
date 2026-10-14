namespace Acme.Workforce.Compliance.Tests.Exception.Rule;

using Acme.Workforce.Compliance.Exception.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class BatchedExceptionRecordChecks
{
    public static void BatchedExceptionRecordStartsInOpen()
    {
        var item = BatchedExceptionRecord.NewBatchedExceptionRecord("code-361", "name-538");
        Check.Equal(item.Status, BatchedExceptionRecordStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedExceptionRecordMovesToResolved()
    {
        var item = BatchedExceptionRecord.NewBatchedExceptionRecord("code-361", "name-538");
        try
        {
            item.MoveTo(BatchedExceptionRecordStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedExceptionRecordStatus.Resolved, "status");
    }

    public static void BatchedExceptionRecordRejectsMovesFromArchived()
    {
        var item = BatchedExceptionRecord.NewBatchedExceptionRecord("code-361", "name-538");
        item.Status = BatchedExceptionRecordStatus.Archived;
        Check.Fails(() => item.MoveTo(BatchedExceptionRecordStatus.Open), "move");
    }

    public static void RunAll()
    {
        BatchedExceptionRecordStartsInOpen();
        BatchedExceptionRecordMovesToResolved();
        BatchedExceptionRecordRejectsMovesFromArchived();
    }
}
