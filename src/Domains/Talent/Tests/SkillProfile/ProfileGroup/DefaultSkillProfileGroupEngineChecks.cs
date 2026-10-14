namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileGroup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SkillProfile.ProfileGroup;

/// <summary>Checks of profile group code, run by the project's runner.</summary>
public static class DefaultSkillProfileGroupEngineChecks
{
    public static void DefaultSkillProfileGroupEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultSkillProfileGroupEngine.DefaultDefaultSkillProfileGroupEngine(Flags.NewRegistry());
        var item = DefaultSkillProfileGroup.NewDefaultSkillProfileGroup("code-197", "label-335");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultSkillProfileGroupEngineKeepsResultsWithinBounds();
    }
}
