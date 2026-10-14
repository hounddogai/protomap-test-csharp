namespace Acme.Workforce.Payrollops.Tests.Garnishment.Mapping;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Garnishment.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class AdaptiveGarnishmentBudgetEngineChecks
{
    public static void AdaptiveGarnishmentBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveGarnishmentBudgetEngine.DefaultAdaptiveGarnishmentBudgetEngine(Flags.NewRegistry());
        var item = AdaptiveGarnishmentBudget.NewAdaptiveGarnishmentBudget("code-457", "title-736");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveGarnishmentBudgetEngineKeepsResultsWithinBounds();
    }
}
