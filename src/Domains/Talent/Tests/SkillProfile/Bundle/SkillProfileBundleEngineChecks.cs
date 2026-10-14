namespace Acme.Workforce.Talent.Tests.SkillProfile.Bundle;

using Acme.Workforce.Talent.SkillProfile.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class SkillProfileBundleEngineChecks
{
    public static void SkillProfileBundleEngineKeepsResultsWithinBounds()
    {
        var engine = SkillProfileBundleEngine.DefaultSkillProfileBundleEngine();
        var item = SkillProfileBundle.NewSkillProfileBundle("code-230", "label-715");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SkillProfileBundleEngineKeepsResultsWithinBounds();
    }
}
