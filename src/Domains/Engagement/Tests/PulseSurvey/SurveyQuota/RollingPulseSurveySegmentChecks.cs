namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyQuota;

using Acme.Workforce.Engagement.PulseSurvey.SurveyQuota;

/// <summary>Checks of survey quota code, run by the project's runner.</summary>
public static class RollingPulseSurveySegmentChecks
{
    public static void RollingPulseSurveySegmentStartsInPlanned()
    {
        var item = RollingPulseSurveySegment.NewRollingPulseSurveySegment("code-340", "label-650");
        Check.Equal(item.Status, RollingPulseSurveySegmentStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingPulseSurveySegmentMovesToCancelled()
    {
        var item = RollingPulseSurveySegment.NewRollingPulseSurveySegment("code-340", "label-650");
        try
        {
            item.MoveTo(RollingPulseSurveySegmentStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingPulseSurveySegmentStatus.Cancelled, "status");
    }

    public static void RollingPulseSurveySegmentRejectsMovesFromCancelled()
    {
        var item = RollingPulseSurveySegment.NewRollingPulseSurveySegment("code-340", "label-650");
        item.Status = RollingPulseSurveySegmentStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RollingPulseSurveySegmentStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RollingPulseSurveySegmentStartsInPlanned();
        RollingPulseSurveySegmentMovesToCancelled();
        RollingPulseSurveySegmentRejectsMovesFromCancelled();
    }
}
