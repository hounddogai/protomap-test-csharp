namespace Acme.Workforce.Talent.Tests.Pool.Budget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Pool.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class PoolBudgetEngineChecks
{
    public static void PoolBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = PoolBudgetEngine.DefaultPoolBudgetEngine(Flags.NewRegistry());
        var item = PoolBudget.NewPoolBudget("code-372", "name-376");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PoolBudgetEngineKeepsResultsWithinBounds();
    }
}
