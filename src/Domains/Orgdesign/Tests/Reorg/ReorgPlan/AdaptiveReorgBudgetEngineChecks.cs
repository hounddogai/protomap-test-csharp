namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgPlan;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

/// <summary>Checks of reorg plan code, run by the project's runner.</summary>
public static class AdaptiveReorgBudgetEngineChecks
{
    public static void AdaptiveReorgBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveReorgBudgetEngine.DefaultAdaptiveReorgBudgetEngine(Flags.NewRegistry());
        var item = AdaptiveReorgBudget.NewAdaptiveReorgBudget("code-881", "title-898");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveReorgBudgetEngineKeepsResultsWithinBounds();
    }
}
