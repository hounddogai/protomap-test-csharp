namespace Acme.Workforce.Talent.Tests.SkillProfile.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SkillProfile.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class SkillProfileAuditEngineChecks
{
    public static void SkillProfileAuditEngineKeepsResultsWithinBounds()
    {
        var engine = SkillProfileAuditEngine.DefaultSkillProfileAuditEngine(Flags.NewRegistry());
        var item = SkillProfileAudit.NewSkillProfileAudit("code-741", "title-493");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SkillProfileAuditEngineKeepsResultsWithinBounds();
    }
}
