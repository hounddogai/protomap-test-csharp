namespace Acme.Workforce.Compensation.Tests.PayBand.BandThreshold;

using Acme.Workforce.Compensation.PayBand.BandThreshold;

/// <summary>Checks of band threshold code, run by the project's runner.</summary>
public static class BatchedPayBandThresholdChecks
{
    public static void BatchedPayBandThresholdStartsInRequested()
    {
        var item = BatchedPayBandThreshold.NewBatchedPayBandThreshold("code-947", "name-525");
        Check.Equal(item.Status, BatchedPayBandThresholdStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedPayBandThresholdMovesToExpired()
    {
        var item = BatchedPayBandThreshold.NewBatchedPayBandThreshold("code-947", "name-525");
        try
        {
            item.MoveTo(BatchedPayBandThresholdStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedPayBandThresholdStatus.Expired, "status");
    }

    public static void BatchedPayBandThresholdRejectsMovesFromExpired()
    {
        var item = BatchedPayBandThreshold.NewBatchedPayBandThreshold("code-947", "name-525");
        item.Status = BatchedPayBandThresholdStatus.Expired;
        Check.Fails(() => item.MoveTo(BatchedPayBandThresholdStatus.Requested), "move");
    }

    public static void RunAll()
    {
        BatchedPayBandThresholdStartsInRequested();
        BatchedPayBandThresholdMovesToExpired();
        BatchedPayBandThresholdRejectsMovesFromExpired();
    }
}
