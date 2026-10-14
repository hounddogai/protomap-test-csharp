namespace Acme.Workforce.Compensation.Tests.EquityGrant.Exception;

using Acme.Workforce.Compensation.EquityGrant.Exception;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class EquityGrantExceptionEngineChecks
{
    public static void EquityGrantExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = EquityGrantExceptionEngine.DefaultEquityGrantExceptionEngine(Flags.NewRegistry());
        var item = EquityGrantException.NewEquityGrantException("code-711", "name-293");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EquityGrantExceptionEngineKeepsResultsWithinBounds();
    }
}
