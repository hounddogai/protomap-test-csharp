namespace Acme.Workforce.Talent.Tests.Nomination.Score;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Nomination.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class StrictNominationScoreEngineChecks
{
    public static void StrictNominationScoreEngineKeepsResultsWithinBounds()
    {
        var engine = StrictNominationScoreEngine.DefaultStrictNominationScoreEngine(Flags.NewRegistry());
        var item = StrictNominationScore.NewStrictNominationScore("code-880", "name-318");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictNominationScoreEngineKeepsResultsWithinBounds();
    }
}
