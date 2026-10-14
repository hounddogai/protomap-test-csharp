namespace Acme.Workforce.Compensation.Tests.MeritCycle.Draft;

using Acme.Workforce.Compensation.MeritCycle.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class MeritCycleTemplateEngineChecks
{
    public static void MeritCycleTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = MeritCycleTemplateEngine.DefaultMeritCycleTemplateEngine();
        var item = MeritCycleTemplate.NewMeritCycleTemplate("code-941", "title-975");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MeritCycleTemplateEngineKeepsResultsWithinBounds();
    }
}
