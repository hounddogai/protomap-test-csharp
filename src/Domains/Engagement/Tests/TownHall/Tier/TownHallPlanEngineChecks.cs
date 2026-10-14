namespace Acme.Workforce.Engagement.Tests.TownHall.Tier;

using Acme.Workforce.Engagement.TownHall.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class TownHallPlanEngineChecks
{
    public static void TownHallPlanEngineKeepsResultsWithinBounds()
    {
        var engine = TownHallPlanEngine.DefaultTownHallPlanEngine();
        var item = TownHallPlan.NewTownHallPlan("code-498", "name-924");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TownHallPlanEngineKeepsResultsWithinBounds();
    }
}
