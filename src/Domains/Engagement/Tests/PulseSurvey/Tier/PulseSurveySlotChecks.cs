namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Tier;

using Acme.Workforce.Engagement.PulseSurvey.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class PulseSurveySlotChecks
{
    public static void PulseSurveySlotStartsInNew()
    {
        var item = PulseSurveySlot.NewPulseSurveySlot("code-628", "name-637");
        Check.Equal(item.Status, PulseSurveySlotStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PulseSurveySlotMovesToDone()
    {
        var item = PulseSurveySlot.NewPulseSurveySlot("code-628", "name-637");
        try
        {
            item.MoveTo(PulseSurveySlotStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PulseSurveySlotStatus.Done, "status");
    }

    public static void PulseSurveySlotRejectsMovesFromFailed()
    {
        var item = PulseSurveySlot.NewPulseSurveySlot("code-628", "name-637");
        item.Status = PulseSurveySlotStatus.Failed;
        Check.Fails(() => item.MoveTo(PulseSurveySlotStatus.New), "move");
    }

    public static void RunAll()
    {
        PulseSurveySlotStartsInNew();
        PulseSurveySlotMovesToDone();
        PulseSurveySlotRejectsMovesFromFailed();
    }
}
