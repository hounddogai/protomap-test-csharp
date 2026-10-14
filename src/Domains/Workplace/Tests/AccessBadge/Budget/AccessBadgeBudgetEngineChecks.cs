namespace Acme.Workforce.Workplace.Tests.AccessBadge.Budget;

using Acme.Workforce.Workplace.AccessBadge.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class AccessBadgeBudgetEngineChecks
{
    public static void AccessBadgeBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = AccessBadgeBudgetEngine.DefaultAccessBadgeBudgetEngine();
        var item = AccessBadgeBudget.NewAccessBadgeBudget("code-754", "name-141");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccessBadgeBudgetEngineKeepsResultsWithinBounds();
    }
}
