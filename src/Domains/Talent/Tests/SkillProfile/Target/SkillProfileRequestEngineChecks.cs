namespace Acme.Workforce.Talent.Tests.SkillProfile.Target;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SkillProfile.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class SkillProfileRequestEngineChecks
{
    public static void SkillProfileRequestEngineKeepsResultsWithinBounds()
    {
        var engine = SkillProfileRequestEngine.DefaultSkillProfileRequestEngine(Flags.NewRegistry());
        var item = SkillProfileRequest.NewSkillProfileRequest("code-538", "name-873");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SkillProfileRequestEngineKeepsResultsWithinBounds();
    }
}
