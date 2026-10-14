namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Claim;

using Acme.Workforce.Engagement.PulseSurvey.Claim;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class PendingPulseSurveyClaimEngineChecks
{
    public static void PendingPulseSurveyClaimEngineKeepsResultsWithinBounds()
    {
        var engine = PendingPulseSurveyClaimEngine.DefaultPendingPulseSurveyClaimEngine(Flags.NewRegistry());
        var item = PendingPulseSurveyClaim.NewPendingPulseSurveyClaim("code-9", "name-572");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingPulseSurveyClaimEngineKeepsResultsWithinBounds();
    }
}
