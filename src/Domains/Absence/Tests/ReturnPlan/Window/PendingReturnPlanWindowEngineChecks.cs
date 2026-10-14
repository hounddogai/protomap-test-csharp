namespace Acme.Workforce.Absence.Tests.ReturnPlan.Window;

using Acme.Workforce.Absence.ReturnPlan.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class PendingReturnPlanWindowEngineChecks
{
    public static void PendingReturnPlanWindowEngineKeepsResultsWithinBounds()
    {
        var engine = PendingReturnPlanWindowEngine.DefaultPendingReturnPlanWindowEngine();
        var item = PendingReturnPlanWindow.NewPendingReturnPlanWindow("code-463", "label-771");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingReturnPlanWindowEngineKeepsResultsWithinBounds();
    }
}
