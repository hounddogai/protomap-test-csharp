namespace Acme.Workforce.Workplace.Tests.Visitor.Bundle;

using Acme.Workforce.Workplace.Visitor.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class VisitorBundleEngineChecks
{
    public static void VisitorBundleEngineKeepsResultsWithinBounds()
    {
        var engine = VisitorBundleEngine.DefaultVisitorBundleEngine();
        var item = VisitorBundle.NewVisitorBundle("code-671", "name-590");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VisitorBundleEngineKeepsResultsWithinBounds();
    }
}
