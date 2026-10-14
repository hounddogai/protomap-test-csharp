namespace Acme.Workforce.Compensation.Tests.MeritCycle.Budget;

using Acme.Workforce.Compensation.MeritCycle.Budget;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class CappedMeritCycleBudgetEngineChecks
{
    public static void CappedMeritCycleBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = CappedMeritCycleBudgetEngine.DefaultCappedMeritCycleBudgetEngine(Flags.NewRegistry());
        var item = CappedMeritCycleBudget.NewCappedMeritCycleBudget("code-260", "name-872");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedMeritCycleBudgetEngineKeepsResultsWithinBounds();
    }
}
