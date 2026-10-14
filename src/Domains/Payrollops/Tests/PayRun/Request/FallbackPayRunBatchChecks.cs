namespace Acme.Workforce.Payrollops.Tests.PayRun.Request;

using Acme.Workforce.Payrollops.PayRun.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class FallbackPayRunBatchChecks
{
    public static void FallbackPayRunBatchStartsInOpen()
    {
        var item = FallbackPayRunBatch.NewFallbackPayRunBatch("code-20", "name-79");
        Check.Equal(item.Status, FallbackPayRunBatchStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackPayRunBatchMovesToResolved()
    {
        var item = FallbackPayRunBatch.NewFallbackPayRunBatch("code-20", "name-79");
        try
        {
            item.MoveTo(FallbackPayRunBatchStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackPayRunBatchStatus.Resolved, "status");
    }

    public static void FallbackPayRunBatchRejectsMovesFromArchived()
    {
        var item = FallbackPayRunBatch.NewFallbackPayRunBatch("code-20", "name-79");
        item.Status = FallbackPayRunBatchStatus.Archived;
        Check.Fails(() => item.MoveTo(FallbackPayRunBatchStatus.Open), "move");
    }

    public static void RunAll()
    {
        FallbackPayRunBatchStartsInOpen();
        FallbackPayRunBatchMovesToResolved();
        FallbackPayRunBatchRejectsMovesFromArchived();
    }
}
