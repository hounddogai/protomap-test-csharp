namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyTier;

using Acme.Workforce.Engagement.PulseSurvey.SurveyTier;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of survey tier code, run by the project's runner.</summary>
public static class ManualPulseSurveyTierEngineChecks
{
    public static void ManualPulseSurveyTierEngineKeepsResultsWithinBounds()
    {
        var engine = ManualPulseSurveyTierEngine.DefaultManualPulseSurveyTierEngine(Flags.NewRegistry());
        var item = ManualPulseSurveyTier.NewManualPulseSurveyTier("code-445", "label-314");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualPulseSurveyTierEngineKeepsResultsWithinBounds();
    }
}
