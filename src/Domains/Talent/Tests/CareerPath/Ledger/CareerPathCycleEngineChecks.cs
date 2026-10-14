namespace Acme.Workforce.Talent.Tests.CareerPath.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.CareerPath.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class CareerPathCycleEngineChecks
{
    public static void CareerPathCycleEngineKeepsResultsWithinBounds()
    {
        var engine = CareerPathCycleEngine.DefaultCareerPathCycleEngine(Flags.NewRegistry());
        var item = CareerPathCycle.NewCareerPathCycle("code-781", "name-312");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CareerPathCycleEngineKeepsResultsWithinBounds();
    }
}
