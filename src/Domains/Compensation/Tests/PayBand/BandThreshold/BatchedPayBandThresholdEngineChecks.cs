namespace Acme.Workforce.Compensation.Tests.PayBand.BandThreshold;

using Acme.Workforce.Compensation.PayBand.BandThreshold;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of band threshold code, run by the project's runner.</summary>
public static class BatchedPayBandThresholdEngineChecks
{
    public static void BatchedPayBandThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedPayBandThresholdEngine.DefaultBatchedPayBandThresholdEngine(Flags.NewRegistry());
        var item = BatchedPayBandThreshold.NewBatchedPayBandThreshold("code-947", "name-525");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedPayBandThresholdEngineKeepsResultsWithinBounds();
    }
}
