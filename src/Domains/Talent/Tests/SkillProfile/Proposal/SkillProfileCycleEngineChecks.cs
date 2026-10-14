namespace Acme.Workforce.Talent.Tests.SkillProfile.Proposal;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SkillProfile.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class SkillProfileCycleEngineChecks
{
    public static void SkillProfileCycleEngineKeepsResultsWithinBounds()
    {
        var engine = SkillProfileCycleEngine.DefaultSkillProfileCycleEngine(Flags.NewRegistry());
        var item = SkillProfileCycle.NewSkillProfileCycle("code-608", "name-23");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SkillProfileCycleEngineKeepsResultsWithinBounds();
    }
}
