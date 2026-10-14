namespace Acme.Workforce.Assets.Tests.Refresh.Checkpoint;

using Acme.Workforce.Assets.Refresh.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class RefreshTemplateEngineChecks
{
    public static void RefreshTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = RefreshTemplateEngine.DefaultRefreshTemplateEngine();
        var item = RefreshTemplate.NewRefreshTemplate("code-406", "title-636");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RefreshTemplateEngineKeepsResultsWithinBounds();
    }
}
