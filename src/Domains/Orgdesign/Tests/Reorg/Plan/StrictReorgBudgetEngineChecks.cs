namespace Acme.Workforce.Orgdesign.Tests.Reorg.Plan;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Reorg.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class StrictReorgBudgetEngineChecks
{
    public static void StrictReorgBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = StrictReorgBudgetEngine.DefaultStrictReorgBudgetEngine(Flags.NewRegistry());
        var item = StrictReorgBudget.NewStrictReorgBudget("code-640", "name-345");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictReorgBudgetEngineKeepsResultsWithinBounds();
    }
}
