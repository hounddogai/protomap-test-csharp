namespace Acme.Workforce.Compliance.Tests.Exception.Batch;

using Acme.Workforce.Compliance.Exception.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class ExceptionBatchChecks
{
    public static void ExceptionBatchStartsInDraft()
    {
        var item = ExceptionBatch.NewExceptionBatch("code-631", "title-8");
        Check.Equal(item.Status, ExceptionBatchStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ExceptionBatchMovesToActive()
    {
        var item = ExceptionBatch.NewExceptionBatch("code-631", "title-8");
        try
        {
            item.MoveTo(ExceptionBatchStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ExceptionBatchStatus.Active, "status");
    }

    public static void ExceptionBatchRejectsMovesFromClosed()
    {
        var item = ExceptionBatch.NewExceptionBatch("code-631", "title-8");
        item.Status = ExceptionBatchStatus.Closed;
        Check.Fails(() => item.MoveTo(ExceptionBatchStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ExceptionBatchStartsInDraft();
        ExceptionBatchMovesToActive();
        ExceptionBatchRejectsMovesFromClosed();
    }
}
