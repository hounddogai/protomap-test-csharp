namespace Acme.Workforce.Payrollops.Tests.PayRun.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.PayRun.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RollingPayRunExceptionEngineChecks
{
    public static void RollingPayRunExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = RollingPayRunExceptionEngine.DefaultRollingPayRunExceptionEngine(Flags.NewRegistry());
        var item = RollingPayRunException.NewRollingPayRunException("code-628", "name-722");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingPayRunExceptionEngineKeepsResultsWithinBounds();
    }
}
