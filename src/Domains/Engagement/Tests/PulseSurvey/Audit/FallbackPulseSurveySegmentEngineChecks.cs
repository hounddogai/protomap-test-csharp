namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Audit;

using Acme.Workforce.Engagement.PulseSurvey.Audit;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class FallbackPulseSurveySegmentEngineChecks
{
    public static void FallbackPulseSurveySegmentEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackPulseSurveySegmentEngine.DefaultFallbackPulseSurveySegmentEngine(Flags.NewRegistry());
        var item = FallbackPulseSurveySegment.NewFallbackPulseSurveySegment("code-226", "name-42");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackPulseSurveySegmentEngineKeepsResultsWithinBounds();
    }
}
