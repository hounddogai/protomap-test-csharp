namespace Acme.Workforce.Talent.Tests.Mobility.Tier;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Mobility.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class ScheduledMobilityRequestEngineChecks
{
    public static void ScheduledMobilityRequestEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledMobilityRequestEngine.DefaultScheduledMobilityRequestEngine(Flags.NewRegistry());
        var item = ScheduledMobilityRequest.NewScheduledMobilityRequest("code-289", "name-650");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledMobilityRequestEngineKeepsResultsWithinBounds();
    }
}
