namespace Acme.Workforce.Orgdesign.Tests.Reorg.Group;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Reorg.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ReorgBundleEngineChecks
{
    public static void ReorgBundleEngineKeepsResultsWithinBounds()
    {
        var engine = ReorgBundleEngine.DefaultReorgBundleEngine(Flags.NewRegistry());
        var item = ReorgBundle.NewReorgBundle("code-728", "title-746");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReorgBundleEngineKeepsResultsWithinBounds();
    }
}
