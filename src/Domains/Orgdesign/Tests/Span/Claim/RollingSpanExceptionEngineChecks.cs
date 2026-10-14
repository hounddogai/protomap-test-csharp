namespace Acme.Workforce.Orgdesign.Tests.Span.Claim;

using Acme.Workforce.Orgdesign.Span.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class RollingSpanExceptionEngineChecks
{
    public static void RollingSpanExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = RollingSpanExceptionEngine.DefaultRollingSpanExceptionEngine();
        var item = RollingSpanException.NewRollingSpanException("code-811", "title-160");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingSpanExceptionEngineKeepsResultsWithinBounds();
    }
}
