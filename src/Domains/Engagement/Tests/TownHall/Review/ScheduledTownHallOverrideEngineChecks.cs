namespace Acme.Workforce.Engagement.Tests.TownHall.Review;

using Acme.Workforce.Engagement.TownHall.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class ScheduledTownHallOverrideEngineChecks
{
    public static void ScheduledTownHallOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledTownHallOverrideEngine.DefaultScheduledTownHallOverrideEngine();
        var item = ScheduledTownHallOverride.NewScheduledTownHallOverride("code-988", "title-93");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledTownHallOverrideEngineKeepsResultsWithinBounds();
    }
}
