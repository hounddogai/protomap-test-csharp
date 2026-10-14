namespace Acme.Workforce.Workplace.Tests.Visitor.Group;

using Acme.Workforce.Workplace.Visitor.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class VisitorGroupEngineChecks
{
    public static void VisitorGroupEngineKeepsResultsWithinBounds()
    {
        var engine = VisitorGroupEngine.DefaultVisitorGroupEngine();
        var item = VisitorGroup.NewVisitorGroup("code-644", "title-782");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VisitorGroupEngineKeepsResultsWithinBounds();
    }
}
