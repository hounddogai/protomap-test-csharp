namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Template;

using Acme.Workforce.Engagement.PulseSurvey.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class PulseSurveyNoticeChecks
{
    public static void PulseSurveyNoticeStartsInPlanned()
    {
        var item = PulseSurveyNotice.NewPulseSurveyNotice("code-202", "title-160");
        Check.Equal(item.Status, PulseSurveyNoticeStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PulseSurveyNoticeMovesToCancelled()
    {
        var item = PulseSurveyNotice.NewPulseSurveyNotice("code-202", "title-160");
        try
        {
            item.MoveTo(PulseSurveyNoticeStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PulseSurveyNoticeStatus.Cancelled, "status");
    }

    public static void PulseSurveyNoticeRejectsMovesFromCancelled()
    {
        var item = PulseSurveyNotice.NewPulseSurveyNotice("code-202", "title-160");
        item.Status = PulseSurveyNoticeStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PulseSurveyNoticeStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PulseSurveyNoticeStartsInPlanned();
        PulseSurveyNoticeMovesToCancelled();
        PulseSurveyNoticeRejectsMovesFromCancelled();
    }
}
