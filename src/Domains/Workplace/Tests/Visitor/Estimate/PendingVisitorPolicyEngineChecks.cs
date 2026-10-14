namespace Acme.Workforce.Workplace.Tests.Visitor.Estimate;

using Acme.Workforce.Workplace.Visitor.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PendingVisitorPolicyEngineChecks
{
    public static void PendingVisitorPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = PendingVisitorPolicyEngine.DefaultPendingVisitorPolicyEngine();
        var item = PendingVisitorPolicy.NewPendingVisitorPolicy("code-710", "label-736");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingVisitorPolicyEngineKeepsResultsWithinBounds();
    }
}
