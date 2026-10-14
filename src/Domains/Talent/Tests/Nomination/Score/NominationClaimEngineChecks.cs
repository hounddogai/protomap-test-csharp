namespace Acme.Workforce.Talent.Tests.Nomination.Score;

using Acme.Workforce.Talent.Nomination.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class NominationClaimEngineChecks
{
    public static void NominationClaimEngineKeepsResultsWithinBounds()
    {
        var engine = NominationClaimEngine.DefaultNominationClaimEngine();
        var item = NominationClaim.NewNominationClaim("code-256", "title-307");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        NominationClaimEngineKeepsResultsWithinBounds();
    }
}
