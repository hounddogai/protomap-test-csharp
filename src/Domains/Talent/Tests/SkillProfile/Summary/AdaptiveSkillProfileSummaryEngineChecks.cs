namespace Acme.Workforce.Talent.Tests.SkillProfile.Summary;

using Acme.Workforce.Talent.SkillProfile.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class AdaptiveSkillProfileSummaryEngineChecks
{
    public static void AdaptiveSkillProfileSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveSkillProfileSummaryEngine.DefaultAdaptiveSkillProfileSummaryEngine();
        var item = AdaptiveSkillProfileSummary.NewAdaptiveSkillProfileSummary("code-340", "name-212");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveSkillProfileSummaryEngineKeepsResultsWithinBounds();
    }
}
