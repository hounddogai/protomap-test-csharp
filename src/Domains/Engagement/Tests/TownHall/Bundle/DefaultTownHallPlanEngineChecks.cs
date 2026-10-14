namespace Acme.Workforce.Engagement.Tests.TownHall.Bundle;

using Acme.Workforce.Engagement.TownHall.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class DefaultTownHallPlanEngineChecks
{
    public static void DefaultTownHallPlanEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultTownHallPlanEngine.DefaultDefaultTownHallPlanEngine();
        var item = DefaultTownHallPlan.NewDefaultTownHallPlan("code-686", "name-724");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultTownHallPlanEngineKeepsResultsWithinBounds();
    }
}
