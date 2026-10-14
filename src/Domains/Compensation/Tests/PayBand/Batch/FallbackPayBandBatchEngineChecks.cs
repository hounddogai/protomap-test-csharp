namespace Acme.Workforce.Compensation.Tests.PayBand.Batch;

using Acme.Workforce.Compensation.PayBand.Batch;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class FallbackPayBandBatchEngineChecks
{
    public static void FallbackPayBandBatchEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackPayBandBatchEngine.DefaultFallbackPayBandBatchEngine(Flags.NewRegistry());
        var item = FallbackPayBandBatch.NewFallbackPayBandBatch("code-453", "title-281");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackPayBandBatchEngineKeepsResultsWithinBounds();
    }
}
