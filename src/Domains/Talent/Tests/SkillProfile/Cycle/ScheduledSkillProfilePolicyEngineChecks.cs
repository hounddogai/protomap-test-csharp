namespace Acme.Workforce.Talent.Tests.SkillProfile.Cycle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SkillProfile.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class ScheduledSkillProfilePolicyEngineChecks
{
    public static void ScheduledSkillProfilePolicyEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledSkillProfilePolicyEngine.DefaultScheduledSkillProfilePolicyEngine(Flags.NewRegistry());
        var item = ScheduledSkillProfilePolicy.NewScheduledSkillProfilePolicy("code-84", "label-310");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledSkillProfilePolicyEngineKeepsResultsWithinBounds();
    }
}
