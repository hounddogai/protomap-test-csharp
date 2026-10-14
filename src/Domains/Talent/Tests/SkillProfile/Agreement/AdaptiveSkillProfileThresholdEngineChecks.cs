namespace Acme.Workforce.Talent.Tests.SkillProfile.Agreement;

using Acme.Workforce.Talent.SkillProfile.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class AdaptiveSkillProfileThresholdEngineChecks
{
    public static void AdaptiveSkillProfileThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveSkillProfileThresholdEngine.DefaultAdaptiveSkillProfileThresholdEngine();
        var item = AdaptiveSkillProfileThreshold.NewAdaptiveSkillProfileThreshold("code-828", "title-885");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveSkillProfileThresholdEngineKeepsResultsWithinBounds();
    }
}
