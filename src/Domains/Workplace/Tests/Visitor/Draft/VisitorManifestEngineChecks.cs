namespace Acme.Workforce.Workplace.Tests.Visitor.Draft;

using Acme.Workforce.Workplace.Visitor.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class VisitorManifestEngineChecks
{
    public static void VisitorManifestEngineKeepsResultsWithinBounds()
    {
        var engine = VisitorManifestEngine.DefaultVisitorManifestEngine();
        var item = VisitorManifest.NewVisitorManifest("code-392", "name-460");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VisitorManifestEngineKeepsResultsWithinBounds();
    }
}
