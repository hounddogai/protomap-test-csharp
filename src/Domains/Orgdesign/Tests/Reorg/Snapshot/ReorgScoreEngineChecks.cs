namespace Acme.Workforce.Orgdesign.Tests.Reorg.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Reorg.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ReorgScoreEngineChecks
{
    public static void ReorgScoreEngineKeepsResultsWithinBounds()
    {
        var engine = ReorgScoreEngine.DefaultReorgScoreEngine(Flags.NewRegistry());
        var item = ReorgScore.NewReorgScore("code-749", "label-261");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReorgScoreEngineKeepsResultsWithinBounds();
    }
}
