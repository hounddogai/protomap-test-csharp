namespace Acme.Workforce.Talent.Tests.Nomination.Rollup;

using Acme.Workforce.Talent.Nomination.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class FallbackNominationExceptionEngineChecks
{
    public static void FallbackNominationExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackNominationExceptionEngine.DefaultFallbackNominationExceptionEngine();
        var item = FallbackNominationException.NewFallbackNominationException("code-462", "name-787");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackNominationExceptionEngineKeepsResultsWithinBounds();
    }
}
