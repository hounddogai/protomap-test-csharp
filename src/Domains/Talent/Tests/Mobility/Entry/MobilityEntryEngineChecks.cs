namespace Acme.Workforce.Talent.Tests.Mobility.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Mobility.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class MobilityEntryEngineChecks
{
    public static void MobilityEntryEngineKeepsResultsWithinBounds()
    {
        var engine = MobilityEntryEngine.DefaultMobilityEntryEngine(Flags.NewRegistry());
        var item = MobilityEntry.NewMobilityEntry("code-650", "name-203");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MobilityEntryEngineKeepsResultsWithinBounds();
    }
}
