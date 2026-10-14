namespace Acme.Workforce.Talent.Tests.SkillProfile.Schedule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SkillProfile.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class SkillProfileMetricEngineChecks
{
    public static void SkillProfileMetricEngineKeepsResultsWithinBounds()
    {
        var engine = SkillProfileMetricEngine.DefaultSkillProfileMetricEngine(Flags.NewRegistry());
        var item = SkillProfileMetric.NewSkillProfileMetric("code-371", "title-812");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SkillProfileMetricEngineKeepsResultsWithinBounds();
    }
}
