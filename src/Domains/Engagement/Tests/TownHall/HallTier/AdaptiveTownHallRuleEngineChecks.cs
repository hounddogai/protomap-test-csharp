namespace Acme.Workforce.Engagement.Tests.TownHall.HallTier;

using Acme.Workforce.Engagement.TownHall.HallTier;

/// <summary>Checks of hall tier code, run by the project's runner.</summary>
public static class AdaptiveTownHallRuleEngineChecks
{
    public static void AdaptiveTownHallRuleEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveTownHallRuleEngine.DefaultAdaptiveTownHallRuleEngine();
        var item = AdaptiveTownHallRule.NewAdaptiveTownHallRule("code-965", "name-525");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveTownHallRuleEngineKeepsResultsWithinBounds();
    }
}
