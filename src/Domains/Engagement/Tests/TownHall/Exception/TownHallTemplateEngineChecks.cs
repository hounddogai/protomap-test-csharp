namespace Acme.Workforce.Engagement.Tests.TownHall.Exception;

using Acme.Workforce.Engagement.TownHall.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class TownHallTemplateEngineChecks
{
    public static void TownHallTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = TownHallTemplateEngine.DefaultTownHallTemplateEngine();
        var item = TownHallTemplate.NewTownHallTemplate("code-796", "label-155");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TownHallTemplateEngineKeepsResultsWithinBounds();
    }
}
