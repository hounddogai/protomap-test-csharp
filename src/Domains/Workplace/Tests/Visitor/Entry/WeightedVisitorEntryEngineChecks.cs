namespace Acme.Workforce.Workplace.Tests.Visitor.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Visitor.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class WeightedVisitorEntryEngineChecks
{
    public static void WeightedVisitorEntryEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedVisitorEntryEngine.DefaultWeightedVisitorEntryEngine(Flags.NewRegistry());
        var item = WeightedVisitorEntry.NewWeightedVisitorEntry("code-953", "name-789");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedVisitorEntryEngineKeepsResultsWithinBounds();
    }
}
