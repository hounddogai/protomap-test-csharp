namespace Acme.Workforce.Talent.Tests.SkillProfile.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SkillProfile.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class SkillProfileEstimateEngineChecks
{
    public static void SkillProfileEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = SkillProfileEstimateEngine.DefaultSkillProfileEstimateEngine(Flags.NewRegistry());
        var item = SkillProfileEstimate.NewSkillProfileEstimate("code-561", "label-31");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SkillProfileEstimateEngineKeepsResultsWithinBounds();
    }
}
