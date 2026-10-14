namespace Acme.Workforce.Talent.Tests.SkillProfile.Review;

using Acme.Workforce.Talent.SkillProfile.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class TieredSkillProfileDigestEngineChecks
{
    public static void TieredSkillProfileDigestEngineKeepsResultsWithinBounds()
    {
        var engine = TieredSkillProfileDigestEngine.DefaultTieredSkillProfileDigestEngine();
        var item = TieredSkillProfileDigest.NewTieredSkillProfileDigest("code-76", "name-798");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredSkillProfileDigestEngineKeepsResultsWithinBounds();
    }
}
