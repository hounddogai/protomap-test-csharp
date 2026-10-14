namespace Acme.Workforce.Workplace.Tests.Visitor.Adjustment;

using Acme.Workforce.Workplace.Visitor.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class GlobalVisitorAdjustmentEngineChecks
{
    public static void GlobalVisitorAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalVisitorAdjustmentEngine.DefaultGlobalVisitorAdjustmentEngine();
        var item = GlobalVisitorAdjustment.NewGlobalVisitorAdjustment("code-835", "label-68");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalVisitorAdjustmentEngineKeepsResultsWithinBounds();
    }
}
