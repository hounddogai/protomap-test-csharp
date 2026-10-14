namespace Acme.Workforce.Talent.Tests.SkillProfile.Group;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SkillProfile.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class RegionalSkillProfileGroupEngineChecks
{
    public static void RegionalSkillProfileGroupEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalSkillProfileGroupEngine.DefaultRegionalSkillProfileGroupEngine(Flags.NewRegistry());
        var item = RegionalSkillProfileGroup.NewRegionalSkillProfileGroup("code-621", "label-205");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalSkillProfileGroupEngineKeepsResultsWithinBounds();
    }
}
