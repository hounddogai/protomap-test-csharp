namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Group;

using Acme.Workforce.Engagement.PulseSurvey.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PulseSurveyGroupChecks
{
    public static void PulseSurveyGroupStartsInDraft()
    {
        var item = PulseSurveyGroup.NewPulseSurveyGroup("code-13", "label-732");
        Check.Equal(item.Status, PulseSurveyGroupStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PulseSurveyGroupMovesToSuspended()
    {
        var item = PulseSurveyGroup.NewPulseSurveyGroup("code-13", "label-732");
        try
        {
            item.MoveTo(PulseSurveyGroupStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PulseSurveyGroupStatus.Suspended, "status");
    }

    public static void PulseSurveyGroupRejectsMovesFromClosed()
    {
        var item = PulseSurveyGroup.NewPulseSurveyGroup("code-13", "label-732");
        item.Status = PulseSurveyGroupStatus.Closed;
        Check.Fails(() => item.MoveTo(PulseSurveyGroupStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PulseSurveyGroupStartsInDraft();
        PulseSurveyGroupMovesToSuspended();
        PulseSurveyGroupRejectsMovesFromClosed();
    }
}
