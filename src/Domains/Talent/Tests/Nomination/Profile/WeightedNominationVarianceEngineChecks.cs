namespace Acme.Workforce.Talent.Tests.Nomination.Profile;

using Acme.Workforce.Talent.Nomination.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class WeightedNominationVarianceEngineChecks
{
    public static void WeightedNominationVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedNominationVarianceEngine.DefaultWeightedNominationVarianceEngine();
        var item = WeightedNominationVariance.NewWeightedNominationVariance("code-706", "title-245");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedNominationVarianceEngineKeepsResultsWithinBounds();
    }
}
