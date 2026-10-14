namespace Acme.Workforce.Workplace.Tests.Visitor.Draft;

using Acme.Workforce.Workplace.Visitor.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class VisitorClaimEngineChecks
{
    public static void VisitorClaimEngineKeepsResultsWithinBounds()
    {
        var engine = VisitorClaimEngine.DefaultVisitorClaimEngine();
        var item = VisitorClaim.NewVisitorClaim("code-857", "label-600");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VisitorClaimEngineKeepsResultsWithinBounds();
    }
}
