namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Quota;

using Acme.Workforce.Engagement.PulseSurvey.Quota;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class ManualPulseSurveyQuotaEngineChecks
{
    public static void ManualPulseSurveyQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = ManualPulseSurveyQuotaEngine.DefaultManualPulseSurveyQuotaEngine(Flags.NewRegistry());
        var item = ManualPulseSurveyQuota.NewManualPulseSurveyQuota("code-670", "name-524");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualPulseSurveyQuotaEngineKeepsResultsWithinBounds();
    }
}
