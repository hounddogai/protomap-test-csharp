namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SuccessionPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class SeasonalSuccessionPlanPreferenceEngineChecks
{
    public static void SeasonalSuccessionPlanPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalSuccessionPlanPreferenceEngine.DefaultSeasonalSuccessionPlanPreferenceEngine(Flags.NewRegistry());
        var item = SeasonalSuccessionPlanPreference.NewSeasonalSuccessionPlanPreference("code-766", "title-55");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalSuccessionPlanPreferenceEngineKeepsResultsWithinBounds();
    }
}
