namespace Acme.Workforce.Workplace.Tests.Visitor.Baseline;

using Acme.Workforce.Workplace.Visitor.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class VisitorBaselineEngineChecks
{
    public static void VisitorBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = VisitorBaselineEngine.DefaultVisitorBaselineEngine();
        var item = VisitorBaseline.NewVisitorBaseline("code-720", "title-31");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VisitorBaselineEngineKeepsResultsWithinBounds();
    }
}
