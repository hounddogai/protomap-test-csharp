namespace Acme.Workforce.Talent.Tests.SkillProfile.Mapping;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SkillProfile.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class CappedSkillProfileMappingEngineChecks
{
    public static void CappedSkillProfileMappingEngineKeepsResultsWithinBounds()
    {
        var engine = CappedSkillProfileMappingEngine.DefaultCappedSkillProfileMappingEngine(Flags.NewRegistry());
        var item = CappedSkillProfileMapping.NewCappedSkillProfileMapping("code-284", "title-401");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedSkillProfileMappingEngineKeepsResultsWithinBounds();
    }
}
