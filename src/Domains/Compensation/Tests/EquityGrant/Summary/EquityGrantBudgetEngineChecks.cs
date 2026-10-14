namespace Acme.Workforce.Compensation.Tests.EquityGrant.Summary;

using Acme.Workforce.Compensation.EquityGrant.Summary;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class EquityGrantBudgetEngineChecks
{
    public static void EquityGrantBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = EquityGrantBudgetEngine.DefaultEquityGrantBudgetEngine(Flags.NewRegistry());
        var item = EquityGrantBudget.NewEquityGrantBudget("code-369", "label-18");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EquityGrantBudgetEngineKeepsResultsWithinBounds();
    }
}
