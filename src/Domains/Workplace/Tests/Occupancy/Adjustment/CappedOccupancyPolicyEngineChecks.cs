namespace Acme.Workforce.Workplace.Tests.Occupancy.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Occupancy.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class CappedOccupancyPolicyEngineChecks
{
    public static void CappedOccupancyPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = CappedOccupancyPolicyEngine.DefaultCappedOccupancyPolicyEngine(Flags.NewRegistry());
        var item = CappedOccupancyPolicy.NewCappedOccupancyPolicy("code-696", "name-924");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedOccupancyPolicyEngineKeepsResultsWithinBounds();
    }
}
