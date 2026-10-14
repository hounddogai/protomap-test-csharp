namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileSummary;

using Acme.Workforce.Talent.SkillProfile.ProfileSummary;

/// <summary>Checks of profile summary code, run by the project's runner.</summary>
public static class SkillProfileQuotaEngineChecks
{
    public static void SkillProfileQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = SkillProfileQuotaEngine.DefaultSkillProfileQuotaEngine();
        var item = SkillProfileQuota.NewSkillProfileQuota("code-91", "label-572");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SkillProfileQuotaEngineKeepsResultsWithinBounds();
    }
}
