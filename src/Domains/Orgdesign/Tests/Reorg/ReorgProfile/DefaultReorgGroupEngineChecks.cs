namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgProfile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

/// <summary>Checks of reorg profile code, run by the project's runner.</summary>
public static class DefaultReorgGroupEngineChecks
{
    public static void DefaultReorgGroupEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultReorgGroupEngine.DefaultDefaultReorgGroupEngine(Flags.NewRegistry());
        var item = DefaultReorgGroup.NewDefaultReorgGroup("code-997", "label-95");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultReorgGroupEngineKeepsResultsWithinBounds();
    }
}
