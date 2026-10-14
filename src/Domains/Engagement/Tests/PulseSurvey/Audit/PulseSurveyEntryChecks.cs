namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Audit;

using Acme.Workforce.Engagement.PulseSurvey.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class PulseSurveyEntryChecks
{
    public static void PulseSurveyEntryStartsInPlanned()
    {
        var item = PulseSurveyEntry.NewPulseSurveyEntry("code-242", "title-420");
        Check.Equal(item.Status, PulseSurveyEntryStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PulseSurveyEntryMovesToInProgress()
    {
        var item = PulseSurveyEntry.NewPulseSurveyEntry("code-242", "title-420");
        try
        {
            item.MoveTo(PulseSurveyEntryStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PulseSurveyEntryStatus.InProgress, "status");
    }

    public static void PulseSurveyEntryRejectsMovesFromCancelled()
    {
        var item = PulseSurveyEntry.NewPulseSurveyEntry("code-242", "title-420");
        item.Status = PulseSurveyEntryStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PulseSurveyEntryStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PulseSurveyEntryStartsInPlanned();
        PulseSurveyEntryMovesToInProgress();
        PulseSurveyEntryRejectsMovesFromCancelled();
    }
}
