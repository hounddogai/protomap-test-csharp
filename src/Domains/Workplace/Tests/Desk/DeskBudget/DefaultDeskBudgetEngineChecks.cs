namespace Acme.Workforce.Workplace.Tests.Desk.DeskBudget;

using Acme.Workforce.Workplace.Desk.DeskBudget;

/// <summary>Checks of desk budget code, run by the project's runner.</summary>
public static class DefaultDeskBudgetEngineChecks
{
    public static void DefaultDeskBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultDeskBudgetEngine.DefaultDefaultDeskBudgetEngine();
        var item = DefaultDeskBudget.NewDefaultDeskBudget("code-175", "title-303");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultDeskBudgetEngineKeepsResultsWithinBounds();
    }
}
