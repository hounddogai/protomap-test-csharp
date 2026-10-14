namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Window;

using Acme.Workforce.Talent.SuccessionPlan.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class SuccessionPlanWindowEngineChecks
{
    public static void SuccessionPlanWindowEngineKeepsResultsWithinBounds()
    {
        var engine = SuccessionPlanWindowEngine.DefaultSuccessionPlanWindowEngine();
        var item = SuccessionPlanWindow.NewSuccessionPlanWindow("code-605", "label-213");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SuccessionPlanWindowEngineKeepsResultsWithinBounds();
    }
}
