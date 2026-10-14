namespace Acme.Workforce.Talent.Tests.SkillProfile.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SkillProfile.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class SkillProfileGroupEngineChecks
{
    public static void SkillProfileGroupEngineKeepsResultsWithinBounds()
    {
        var engine = SkillProfileGroupEngine.DefaultSkillProfileGroupEngine(Flags.NewRegistry());
        var item = SkillProfileGroup.NewSkillProfileGroup("code-986", "name-326");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SkillProfileGroupEngineKeepsResultsWithinBounds();
    }
}
