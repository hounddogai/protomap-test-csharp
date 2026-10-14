namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgBaseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Reorg.ReorgBaseline;

/// <summary>Checks of reorg baseline code, run by the project's runner.</summary>
public static class ReorgNoticeEngineChecks
{
    public static void ReorgNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = ReorgNoticeEngine.DefaultReorgNoticeEngine(Flags.NewRegistry());
        var item = ReorgNotice.NewReorgNotice("code-200", "title-493");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReorgNoticeEngineKeepsResultsWithinBounds();
    }
}
