namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Threshold;

using Acme.Workforce.Engagement.PulseSurvey.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PendingPulseSurveyAgreementEngineChecks
{
    public static void PendingPulseSurveyAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = PendingPulseSurveyAgreementEngine.DefaultPendingPulseSurveyAgreementEngine();
        var item = PendingPulseSurveyAgreement.NewPendingPulseSurveyAgreement("code-308", "title-461");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingPulseSurveyAgreementEngineKeepsResultsWithinBounds();
    }
}
