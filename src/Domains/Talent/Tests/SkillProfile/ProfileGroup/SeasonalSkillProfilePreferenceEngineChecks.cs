namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileGroup;

using Acme.Workforce.Talent.SkillProfile.ProfileGroup;

/// <summary>Checks of profile group code, run by the project's runner.</summary>
public static class SeasonalSkillProfilePreferenceEngineChecks
{
    public static void SeasonalSkillProfilePreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalSkillProfilePreferenceEngine.DefaultSeasonalSkillProfilePreferenceEngine();
        var item = SeasonalSkillProfilePreference.NewSeasonalSkillProfilePreference("code-293", "title-427");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalSkillProfilePreferenceEngineKeepsResultsWithinBounds();
    }
}
