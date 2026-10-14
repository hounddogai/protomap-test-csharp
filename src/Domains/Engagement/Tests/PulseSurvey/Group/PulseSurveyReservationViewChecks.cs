namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Group;

using Acme.Workforce.Engagement.PulseSurvey.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PulseSurveyReservationViewChecks
{
    public static void PulseSurveyReservationViewShowsTheStatusInCapitals()
    {
        var item = PulseSurveyReservation.NewPulseSurveyReservation("code-524", "title-309");
        var view = PulseSurveyReservationView.ToPulseSurveyReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PulseSurveyReservationView.ToPulseSurveyReservationViews(new List<PulseSurveyReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PulseSurveyReservationViewShowsTheStatusInCapitals();
    }
}
