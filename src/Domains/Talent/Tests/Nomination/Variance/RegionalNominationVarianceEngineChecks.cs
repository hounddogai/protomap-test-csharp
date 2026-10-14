namespace Acme.Workforce.Talent.Tests.Nomination.Variance;

using Acme.Workforce.Talent.Nomination.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class RegionalNominationVarianceEngineChecks
{
    public static void RegionalNominationVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalNominationVarianceEngine.DefaultRegionalNominationVarianceEngine();
        var item = RegionalNominationVariance.NewRegionalNominationVariance("code-915", "name-703");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalNominationVarianceEngineKeepsResultsWithinBounds();
    }
}
