namespace Acme.Workforce.Talent.Tests.SkillProfile.Group;

using Acme.Workforce.Talent.SkillProfile.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class DefaultSkillProfileScoreEngineChecks
{
    public static void DefaultSkillProfileScoreEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultSkillProfileScoreEngine.DefaultDefaultSkillProfileScoreEngine();
        var item = DefaultSkillProfileScore.NewDefaultSkillProfileScore("code-868", "label-883");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultSkillProfileScoreEngineKeepsResultsWithinBounds();
    }
}
