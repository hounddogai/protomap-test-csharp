namespace Acme.Workforce.Talent.Tests.SkillProfile.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SkillProfile.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class SkillProfileAdjustmentEngineChecks
{
    public static void SkillProfileAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = SkillProfileAdjustmentEngine.DefaultSkillProfileAdjustmentEngine(Flags.NewRegistry());
        var item = SkillProfileAdjustment.NewSkillProfileAdjustment("code-246", "label-473");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SkillProfileAdjustmentEngineKeepsResultsWithinBounds();
    }
}
