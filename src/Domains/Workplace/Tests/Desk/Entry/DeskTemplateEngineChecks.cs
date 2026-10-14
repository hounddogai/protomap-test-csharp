namespace Acme.Workforce.Workplace.Tests.Desk.Entry;

using Acme.Workforce.Workplace.Desk.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class DeskTemplateEngineChecks
{
    public static void DeskTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = DeskTemplateEngine.DefaultDeskTemplateEngine();
        var item = DeskTemplate.NewDeskTemplate("code-183", "label-155");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeskTemplateEngineKeepsResultsWithinBounds();
    }
}
