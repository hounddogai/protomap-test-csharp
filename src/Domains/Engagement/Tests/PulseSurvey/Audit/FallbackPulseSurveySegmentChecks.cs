namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Audit;

using Acme.Workforce.Engagement.PulseSurvey.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class FallbackPulseSurveySegmentChecks
{
    public static void FallbackPulseSurveySegmentStartsInIdle()
    {
        var item = FallbackPulseSurveySegment.NewFallbackPulseSurveySegment("code-226", "name-42");
        Check.Equal(item.Status, FallbackPulseSurveySegmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackPulseSurveySegmentMovesToReady()
    {
        var item = FallbackPulseSurveySegment.NewFallbackPulseSurveySegment("code-226", "name-42");
        try
        {
            item.MoveTo(FallbackPulseSurveySegmentStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackPulseSurveySegmentStatus.Ready, "status");
    }

    public static void FallbackPulseSurveySegmentRejectsMovesFromStopped()
    {
        var item = FallbackPulseSurveySegment.NewFallbackPulseSurveySegment("code-226", "name-42");
        item.Status = FallbackPulseSurveySegmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(FallbackPulseSurveySegmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        FallbackPulseSurveySegmentStartsInIdle();
        FallbackPulseSurveySegmentMovesToReady();
        FallbackPulseSurveySegmentRejectsMovesFromStopped();
    }
}
