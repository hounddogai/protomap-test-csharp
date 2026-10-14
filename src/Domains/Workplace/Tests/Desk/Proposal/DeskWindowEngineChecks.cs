namespace Acme.Workforce.Workplace.Tests.Desk.Proposal;

using Acme.Workforce.Workplace.Desk.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DeskWindowEngineChecks
{
    public static void DeskWindowEngineKeepsResultsWithinBounds()
    {
        var engine = DeskWindowEngine.DefaultDeskWindowEngine();
        var item = DeskWindow.NewDeskWindow("code-598", "name-602");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeskWindowEngineKeepsResultsWithinBounds();
    }
}
