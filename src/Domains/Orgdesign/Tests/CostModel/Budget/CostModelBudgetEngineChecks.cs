namespace Acme.Workforce.Orgdesign.Tests.CostModel.Budget;

using Acme.Workforce.Orgdesign.CostModel.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class CostModelBudgetEngineChecks
{
    public static void CostModelBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = CostModelBudgetEngine.DefaultCostModelBudgetEngine();
        var item = CostModelBudget.NewCostModelBudget("code-186", "title-135");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CostModelBudgetEngineKeepsResultsWithinBounds();
    }
}
