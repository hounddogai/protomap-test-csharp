namespace Acme.Workforce.Contractors.Tests.Rate.Score;

using Acme.Workforce.Contractors.Rate.Score;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class RateExceptionEngineChecks
{
    public static void RateExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = RateExceptionEngine.DefaultRateExceptionEngine(Flags.NewRegistry());
        var item = RateException.NewRateException("code-18", "title-77");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RateExceptionEngineKeepsResultsWithinBounds();
    }
}
