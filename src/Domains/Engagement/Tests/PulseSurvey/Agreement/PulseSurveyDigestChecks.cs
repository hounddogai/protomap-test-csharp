namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Agreement;

using Acme.Workforce.Engagement.PulseSurvey.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class PulseSurveyDigestChecks
{
    public static void PulseSurveyDigestStartsInDraft()
    {
        var item = PulseSurveyDigest.NewPulseSurveyDigest("code-388", "title-805");
        Check.Equal(item.Status, PulseSurveyDigestStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PulseSurveyDigestMovesToPending()
    {
        var item = PulseSurveyDigest.NewPulseSurveyDigest("code-388", "title-805");
        try
        {
            item.MoveTo(PulseSurveyDigestStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PulseSurveyDigestStatus.Pending, "status");
    }

    public static void PulseSurveyDigestRejectsMovesFromClosed()
    {
        var item = PulseSurveyDigest.NewPulseSurveyDigest("code-388", "title-805");
        item.Status = PulseSurveyDigestStatus.Closed;
        Check.Fails(() => item.MoveTo(PulseSurveyDigestStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PulseSurveyDigestStartsInDraft();
        PulseSurveyDigestMovesToPending();
        PulseSurveyDigestRejectsMovesFromClosed();
    }
}
