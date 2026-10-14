namespace Acme.Workforce.Workplace.Tests.Visitor.Cycle;

using Acme.Workforce.Workplace.Visitor.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class VisitorQuotaEngineChecks
{
    public static void VisitorQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = VisitorQuotaEngine.DefaultVisitorQuotaEngine();
        var item = VisitorQuota.NewVisitorQuota("code-494", "label-594");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VisitorQuotaEngineKeepsResultsWithinBounds();
    }
}
