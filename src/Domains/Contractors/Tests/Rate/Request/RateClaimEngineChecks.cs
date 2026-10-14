namespace Acme.Workforce.Contractors.Tests.Rate.Request;

using Acme.Workforce.Contractors.Rate.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class RateClaimEngineChecks
{
    public static void RateClaimEngineKeepsResultsWithinBounds()
    {
        var engine = RateClaimEngine.DefaultRateClaimEngine();
        var item = RateClaim.NewRateClaim("code-259", "name-422");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RateClaimEngineKeepsResultsWithinBounds();
    }
}
