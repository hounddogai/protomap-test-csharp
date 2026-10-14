namespace Acme.Workforce.Talent.Tests.Pool.Exception;

using Acme.Workforce.Talent.Pool.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class GlobalPoolExceptionEngineChecks
{
    public static void GlobalPoolExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalPoolExceptionEngine.DefaultGlobalPoolExceptionEngine();
        var item = GlobalPoolException.NewGlobalPoolException("code-478", "label-721");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalPoolExceptionEngineKeepsResultsWithinBounds();
    }
}
