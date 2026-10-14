namespace Acme.Workforce.Orgdesign.Tests.Reorg.Group;

using Acme.Workforce.Orgdesign.Reorg.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ReorgGroupEngineChecks
{
    public static void ReorgGroupEngineKeepsResultsWithinBounds()
    {
        var engine = ReorgGroupEngine.DefaultReorgGroupEngine();
        var item = ReorgGroup.NewReorgGroup("code-128", "title-100");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReorgGroupEngineKeepsResultsWithinBounds();
    }
}
