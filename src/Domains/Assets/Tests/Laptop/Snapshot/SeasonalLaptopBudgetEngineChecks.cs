namespace Acme.Workforce.Assets.Tests.Laptop.Snapshot;

using Acme.Workforce.Assets.Laptop.Snapshot;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SeasonalLaptopBudgetEngineChecks
{
    public static void SeasonalLaptopBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalLaptopBudgetEngine.DefaultSeasonalLaptopBudgetEngine(Flags.NewRegistry());
        var item = SeasonalLaptopBudget.NewSeasonalLaptopBudget("code-887", "name-748");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalLaptopBudgetEngineKeepsResultsWithinBounds();
    }
}
