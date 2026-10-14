namespace Acme.Workforce.Talent.Tests.SkillProfile.Cycle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SkillProfile.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class SeasonalSkillProfileCycleEngineChecks
{
    public static void SeasonalSkillProfileCycleEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalSkillProfileCycleEngine.DefaultSeasonalSkillProfileCycleEngine(Flags.NewRegistry());
        var item = SeasonalSkillProfileCycle.NewSeasonalSkillProfileCycle("code-787", "label-12");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalSkillProfileCycleEngineKeepsResultsWithinBounds();
    }
}
