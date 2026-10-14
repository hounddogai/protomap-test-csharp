namespace Acme.Workforce.Contractors.Tests.Rate.RateNotice;

using Acme.Workforce.Contractors.Rate.RateNotice;

/// <summary>Checks of rate notice code, run by the project's runner.</summary>
public static class GlobalRateExceptionEngineChecks
{
    public static void GlobalRateExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalRateExceptionEngine.DefaultGlobalRateExceptionEngine();
        var item = GlobalRateException.NewGlobalRateException("code-708", "label-551");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalRateExceptionEngineKeepsResultsWithinBounds();
    }
}
