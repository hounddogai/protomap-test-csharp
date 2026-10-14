namespace Acme.Workforce.Compensation.Tests.PayBand.Batch;

using Acme.Workforce.Compensation.PayBand.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class FallbackPayBandBatchChecks
{
    public static void FallbackPayBandBatchStartsInNew()
    {
        var item = FallbackPayBandBatch.NewFallbackPayBandBatch("code-453", "title-281");
        Check.Equal(item.Status, FallbackPayBandBatchStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackPayBandBatchMovesToDone()
    {
        var item = FallbackPayBandBatch.NewFallbackPayBandBatch("code-453", "title-281");
        try
        {
            item.MoveTo(FallbackPayBandBatchStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackPayBandBatchStatus.Done, "status");
    }

    public static void FallbackPayBandBatchRejectsMovesFromFailed()
    {
        var item = FallbackPayBandBatch.NewFallbackPayBandBatch("code-453", "title-281");
        item.Status = FallbackPayBandBatchStatus.Failed;
        Check.Fails(() => item.MoveTo(FallbackPayBandBatchStatus.New), "move");
    }

    public static void RunAll()
    {
        FallbackPayBandBatchStartsInNew();
        FallbackPayBandBatchMovesToDone();
        FallbackPayBandBatchRejectsMovesFromFailed();
    }
}
