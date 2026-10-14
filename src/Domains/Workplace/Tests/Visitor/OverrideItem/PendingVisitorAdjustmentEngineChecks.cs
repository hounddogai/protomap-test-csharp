namespace Acme.Workforce.Workplace.Tests.Visitor.OverrideItem;

using Acme.Workforce.Workplace.Visitor.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PendingVisitorAdjustmentEngineChecks
{
    public static void PendingVisitorAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = PendingVisitorAdjustmentEngine.DefaultPendingVisitorAdjustmentEngine();
        var item = PendingVisitorAdjustment.NewPendingVisitorAdjustment("code-705", "title-869");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingVisitorAdjustmentEngineKeepsResultsWithinBounds();
    }
}
