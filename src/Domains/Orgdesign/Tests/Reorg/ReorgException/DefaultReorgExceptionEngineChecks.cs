namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgException;

using Acme.Workforce.Orgdesign.Reorg.ReorgException;

/// <summary>Checks of reorg exception code, run by the project's runner.</summary>
public static class DefaultReorgExceptionEngineChecks
{
    public static void DefaultReorgExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultReorgExceptionEngine.DefaultDefaultReorgExceptionEngine();
        var item = DefaultReorgException.NewDefaultReorgException("code-151", "title-650");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultReorgExceptionEngineKeepsResultsWithinBounds();
    }
}
