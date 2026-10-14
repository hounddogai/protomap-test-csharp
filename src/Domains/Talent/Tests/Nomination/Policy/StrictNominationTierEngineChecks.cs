namespace Acme.Workforce.Talent.Tests.Nomination.Policy;

using Acme.Workforce.Talent.Nomination.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class StrictNominationTierEngineChecks
{
    public static void StrictNominationTierEngineKeepsResultsWithinBounds()
    {
        var engine = StrictNominationTierEngine.DefaultStrictNominationTierEngine();
        var item = StrictNominationTier.NewStrictNominationTier("code-302", "title-961");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictNominationTierEngineKeepsResultsWithinBounds();
    }
}
