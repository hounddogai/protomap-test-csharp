namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Threshold;

using Acme.Workforce.Engagement.PulseSurvey.Threshold;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PulseSurveyReviewEngineChecks
{
    public static void PulseSurveyReviewEngineKeepsResultsWithinBounds()
    {
        var engine = PulseSurveyReviewEngine.DefaultPulseSurveyReviewEngine(Flags.NewRegistry());
        var item = PulseSurveyReview.NewPulseSurveyReview("code-703", "label-588");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PulseSurveyReviewEngineKeepsResultsWithinBounds();
    }
}
