namespace Acme.Workforce.Talent.Tests.Nomination.Checkpoint;

using Acme.Workforce.Talent.Nomination.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class NominationThresholdEngineChecks
{
    public static void NominationThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = NominationThresholdEngine.DefaultNominationThresholdEngine();
        var item = NominationThreshold.NewNominationThreshold("code-790", "label-872");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        NominationThresholdEngineKeepsResultsWithinBounds();
    }
}
