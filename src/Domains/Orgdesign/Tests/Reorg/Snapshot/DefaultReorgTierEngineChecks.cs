namespace Acme.Workforce.Orgdesign.Tests.Reorg.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Reorg.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class DefaultReorgTierEngineChecks
{
    public static void DefaultReorgTierEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultReorgTierEngine.DefaultDefaultReorgTierEngine(Flags.NewRegistry());
        var item = DefaultReorgTier.NewDefaultReorgTier("code-974", "label-487");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultReorgTierEngineKeepsResultsWithinBounds();
    }
}
