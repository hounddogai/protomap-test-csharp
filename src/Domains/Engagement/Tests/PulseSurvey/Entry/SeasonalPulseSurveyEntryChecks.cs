namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Entry;

using Acme.Workforce.Engagement.PulseSurvey.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class SeasonalPulseSurveyEntryChecks
{
    public static void SeasonalPulseSurveyEntryStartsInIdle()
    {
        var item = SeasonalPulseSurveyEntry.NewSeasonalPulseSurveyEntry("code-181", "label-858");
        Check.Equal(item.Status, SeasonalPulseSurveyEntryStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalPulseSurveyEntryMovesToWarming()
    {
        var item = SeasonalPulseSurveyEntry.NewSeasonalPulseSurveyEntry("code-181", "label-858");
        try
        {
            item.MoveTo(SeasonalPulseSurveyEntryStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalPulseSurveyEntryStatus.Warming, "status");
    }

    public static void SeasonalPulseSurveyEntryRejectsMovesFromStopped()
    {
        var item = SeasonalPulseSurveyEntry.NewSeasonalPulseSurveyEntry("code-181", "label-858");
        item.Status = SeasonalPulseSurveyEntryStatus.Stopped;
        Check.Fails(() => item.MoveTo(SeasonalPulseSurveyEntryStatus.Idle), "move");
    }

    public static void RunAll()
    {
        SeasonalPulseSurveyEntryStartsInIdle();
        SeasonalPulseSurveyEntryMovesToWarming();
        SeasonalPulseSurveyEntryRejectsMovesFromStopped();
    }
}
