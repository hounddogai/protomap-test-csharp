namespace Acme.Workforce.Engagement.Tests.TownHall.HallRule;

using Acme.Workforce.Engagement.TownHall.HallRule;

/// <summary>Checks of hall rule code, run by the project's runner.</summary>
public static class TownHallDraftEngineChecks
{
    public static void TownHallDraftEngineKeepsResultsWithinBounds()
    {
        var engine = TownHallDraftEngine.DefaultTownHallDraftEngine();
        var item = TownHallDraft.NewTownHallDraft("code-59", "name-464");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TownHallDraftEngineKeepsResultsWithinBounds();
    }
}
