namespace Acme.Workforce.Talent.Tests.Pool.PoolDraft;

using Acme.Workforce.Talent.Pool.PoolDraft;

/// <summary>Checks of pool draft code, run by the project's runner.</summary>
public static class AdaptivePoolDraftEngineChecks
{
    public static void AdaptivePoolDraftEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptivePoolDraftEngine.DefaultAdaptivePoolDraftEngine();
        var item = AdaptivePoolDraft.NewAdaptivePoolDraft("code-869", "label-242");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptivePoolDraftEngineKeepsResultsWithinBounds();
    }
}
