namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyTier;

using Acme.Workforce.Engagement.PulseSurvey.SurveyTier;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of survey tier code, run by the project's runner.</summary>
public static class SeasonalPulseSurveyQuotaEngineChecks
{
    public static void SeasonalPulseSurveyQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalPulseSurveyQuotaEngine.DefaultSeasonalPulseSurveyQuotaEngine(Flags.NewRegistry());
        var item = SeasonalPulseSurveyQuota.NewSeasonalPulseSurveyQuota("code-535", "label-234");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalPulseSurveyQuotaEngineKeepsResultsWithinBounds();
    }
}
