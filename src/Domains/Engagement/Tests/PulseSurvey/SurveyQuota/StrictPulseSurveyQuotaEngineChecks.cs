namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyQuota;

using Acme.Workforce.Engagement.PulseSurvey.SurveyQuota;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of survey quota code, run by the project's runner.</summary>
public static class StrictPulseSurveyQuotaEngineChecks
{
    public static void StrictPulseSurveyQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = StrictPulseSurveyQuotaEngine.DefaultStrictPulseSurveyQuotaEngine(Flags.NewRegistry());
        var item = StrictPulseSurveyQuota.NewStrictPulseSurveyQuota("code-449", "name-167");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictPulseSurveyQuotaEngineKeepsResultsWithinBounds();
    }
}
