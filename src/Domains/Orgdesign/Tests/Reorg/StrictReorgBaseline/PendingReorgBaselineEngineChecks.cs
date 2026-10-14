namespace Acme.Workforce.Orgdesign.Tests.Reorg.StrictReorgBaseline;

using Acme.Workforce.Orgdesign.Reorg.StrictReorgBaseline;

/// <summary>Checks of strict reorg baseline code, run by the project's runner.</summary>
public static class PendingReorgBaselineEngineChecks
{
    public static void PendingReorgBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = PendingReorgBaselineEngine.DefaultPendingReorgBaselineEngine();
        var item = PendingReorgBaseline.NewPendingReorgBaseline("code-565", "label-283");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingReorgBaselineEngineKeepsResultsWithinBounds();
    }
}
