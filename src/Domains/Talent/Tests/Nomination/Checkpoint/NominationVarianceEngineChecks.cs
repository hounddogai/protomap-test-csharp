namespace Acme.Workforce.Talent.Tests.Nomination.Checkpoint;

using Acme.Workforce.Talent.Nomination.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class NominationVarianceEngineChecks
{
    public static void NominationVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = NominationVarianceEngine.DefaultNominationVarianceEngine();
        var item = NominationVariance.NewNominationVariance("code-486", "label-583");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        NominationVarianceEngineKeepsResultsWithinBounds();
    }
}
