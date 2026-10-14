namespace Acme.Workforce.Talent.Tests.SkillProfile.Manifest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SkillProfile.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class SkillProfileSlotEngineChecks
{
    public static void SkillProfileSlotEngineKeepsResultsWithinBounds()
    {
        var engine = SkillProfileSlotEngine.DefaultSkillProfileSlotEngine(Flags.NewRegistry());
        var item = SkillProfileSlot.NewSkillProfileSlot("code-753", "label-841");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SkillProfileSlotEngineKeepsResultsWithinBounds();
    }
}
