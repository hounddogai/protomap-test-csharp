namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileSummary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SkillProfile.ProfileSummary;

/// <summary>Checks of profile summary code, run by the project's runner.</summary>
public static class DefaultSkillProfileDraftEngineChecks
{
    public static void DefaultSkillProfileDraftEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultSkillProfileDraftEngine.DefaultDefaultSkillProfileDraftEngine(Flags.NewRegistry());
        var item = DefaultSkillProfileDraft.NewDefaultSkillProfileDraft("code-307", "name-985");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultSkillProfileDraftEngineKeepsResultsWithinBounds();
    }
}
