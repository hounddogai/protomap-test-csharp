namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Manifest;

using Acme.Workforce.Engagement.PulseSurvey.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class PulseSurveyDraftChecks
{
    public static void PulseSurveyDraftStartsInDraft()
    {
        var item = PulseSurveyDraft.NewPulseSurveyDraft("code-340", "name-453");
        Check.Equal(item.Status, PulseSurveyDraftStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PulseSurveyDraftMovesToClosed()
    {
        var item = PulseSurveyDraft.NewPulseSurveyDraft("code-340", "name-453");
        try
        {
            item.MoveTo(PulseSurveyDraftStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PulseSurveyDraftStatus.Closed, "status");
    }

    public static void PulseSurveyDraftRejectsMovesFromClosed()
    {
        var item = PulseSurveyDraft.NewPulseSurveyDraft("code-340", "name-453");
        item.Status = PulseSurveyDraftStatus.Closed;
        Check.Fails(() => item.MoveTo(PulseSurveyDraftStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PulseSurveyDraftStartsInDraft();
        PulseSurveyDraftMovesToClosed();
        PulseSurveyDraftRejectsMovesFromClosed();
    }
}
