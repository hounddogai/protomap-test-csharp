namespace Acme.Workforce.Talent.Tests.SkillProfile.Forecast;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SkillProfile.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class SkillProfileSegmentEngineChecks
{
    public static void SkillProfileSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = SkillProfileSegmentEngine.DefaultSkillProfileSegmentEngine(Flags.NewRegistry());
        var item = SkillProfileSegment.NewSkillProfileSegment("code-501", "label-779");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SkillProfileSegmentEngineKeepsResultsWithinBounds();
    }
}
