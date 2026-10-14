namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Group;

using Acme.Workforce.Engagement.PulseSurvey.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PulseSurveyReservationChecks
{
    public static void PulseSurveyReservationStartsInCreated()
    {
        var item = PulseSurveyReservation.NewPulseSurveyReservation("code-524", "title-309");
        Check.Equal(item.Status, PulseSurveyReservationStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PulseSurveyReservationMovesToPublished()
    {
        var item = PulseSurveyReservation.NewPulseSurveyReservation("code-524", "title-309");
        try
        {
            item.MoveTo(PulseSurveyReservationStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PulseSurveyReservationStatus.Published, "status");
    }

    public static void PulseSurveyReservationRejectsMovesFromRetired()
    {
        var item = PulseSurveyReservation.NewPulseSurveyReservation("code-524", "title-309");
        item.Status = PulseSurveyReservationStatus.Retired;
        Check.Fails(() => item.MoveTo(PulseSurveyReservationStatus.Created), "move");
    }

    public static void RunAll()
    {
        PulseSurveyReservationStartsInCreated();
        PulseSurveyReservationMovesToPublished();
        PulseSurveyReservationRejectsMovesFromRetired();
    }
}
