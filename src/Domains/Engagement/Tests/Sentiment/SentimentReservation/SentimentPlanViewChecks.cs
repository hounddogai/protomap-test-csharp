namespace Acme.Workforce.Engagement.Tests.Sentiment.SentimentReservation;

using Acme.Workforce.Engagement.Sentiment.SentimentReservation;

/// <summary>Checks of sentiment reservation code, run by the project's runner.</summary>
public static class SentimentPlanViewChecks
{
    public static void SentimentPlanViewShowsTheStatusInCapitals()
    {
        var item = SentimentPlan.NewSentimentPlan("code-375", "label-994");
        var view = SentimentPlanView.ToSentimentPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SentimentPlanView.ToSentimentPlanViews(new List<SentimentPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SentimentPlanViewShowsTheStatusInCapitals();
    }
}
