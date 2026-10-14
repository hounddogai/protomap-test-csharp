namespace Acme.Workforce.Payrollops.Tests.Garnishment.Exception;

using Acme.Workforce.Payrollops.Garnishment.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class BatchedGarnishmentExceptionChecks
{
    public static void BatchedGarnishmentExceptionStartsInNew()
    {
        var item = BatchedGarnishmentException.NewBatchedGarnishmentException("code-274", "title-694");
        Check.Equal(item.Status, BatchedGarnishmentExceptionStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedGarnishmentExceptionMovesToDone()
    {
        var item = BatchedGarnishmentException.NewBatchedGarnishmentException("code-274", "title-694");
        try
        {
            item.MoveTo(BatchedGarnishmentExceptionStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedGarnishmentExceptionStatus.Done, "status");
    }

    public static void BatchedGarnishmentExceptionRejectsMovesFromFailed()
    {
        var item = BatchedGarnishmentException.NewBatchedGarnishmentException("code-274", "title-694");
        item.Status = BatchedGarnishmentExceptionStatus.Failed;
        Check.Fails(() => item.MoveTo(BatchedGarnishmentExceptionStatus.New), "move");
    }

    public static void RunAll()
    {
        BatchedGarnishmentExceptionStartsInNew();
        BatchedGarnishmentExceptionMovesToDone();
        BatchedGarnishmentExceptionRejectsMovesFromFailed();
    }
}
