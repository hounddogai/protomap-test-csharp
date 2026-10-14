namespace Acme.Workforce.Engagement.Tests.Recognition.Request;

using Acme.Workforce.Engagement.Recognition.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class RecognitionReservationViewChecks
{
    public static void RecognitionReservationViewShowsTheStatusInCapitals()
    {
        var item = RecognitionReservation.NewRecognitionReservation("code-804", "title-59");
        var view = RecognitionReservationView.ToRecognitionReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RecognitionReservationView.ToRecognitionReservationViews(new List<RecognitionReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RecognitionReservationViewShowsTheStatusInCapitals();
    }
}
