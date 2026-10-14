namespace Acme.Workforce.Talent.Tests.Pool.PoolRule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Pool.PoolRule;

/// <summary>Checks of pool rule code, run by the project's runner.</summary>
public static class PoolExceptionEngineChecks
{
    public static void PoolExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = PoolExceptionEngine.DefaultPoolExceptionEngine(Flags.NewRegistry());
        var item = PoolException.NewPoolException("code-737", "title-296");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PoolExceptionEngineKeepsResultsWithinBounds();
    }
}
