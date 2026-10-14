namespace Acme.Workforce.Talent.Tests.Nomination.Window;

using Acme.Workforce.Talent.Nomination.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class NominationScoreEngineChecks
{
    public static void NominationScoreEngineKeepsResultsWithinBounds()
    {
        var engine = NominationScoreEngine.DefaultNominationScoreEngine();
        var item = NominationScore.NewNominationScore("code-546", "label-366");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        NominationScoreEngineKeepsResultsWithinBounds();
    }
}
