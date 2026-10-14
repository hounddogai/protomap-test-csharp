namespace Acme.Workforce.Compensation.Tests.PayBand.Estimate;

using Acme.Workforce.Compensation.PayBand.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class FallbackPayBandMappingEngineChecks
{
    public static void FallbackPayBandMappingEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackPayBandMappingEngine.DefaultFallbackPayBandMappingEngine();
        var item = FallbackPayBandMapping.NewFallbackPayBandMapping("code-834", "title-211");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackPayBandMappingEngineKeepsResultsWithinBounds();
    }
}
