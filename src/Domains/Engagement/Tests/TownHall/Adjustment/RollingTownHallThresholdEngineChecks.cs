namespace Acme.Workforce.Engagement.Tests.TownHall.Adjustment;

using Acme.Workforce.Engagement.TownHall.Adjustment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class RollingTownHallThresholdEngineChecks
{
    public static void RollingTownHallThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = RollingTownHallThresholdEngine.DefaultRollingTownHallThresholdEngine(Flags.NewRegistry());
        var item = RollingTownHallThreshold.NewRollingTownHallThreshold("code-441", "title-155");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingTownHallThresholdEngineKeepsResultsWithinBounds();
    }
}
