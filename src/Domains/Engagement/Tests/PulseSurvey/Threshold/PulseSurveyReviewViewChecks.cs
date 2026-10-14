namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Threshold;

using Acme.Workforce.Engagement.PulseSurvey.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PulseSurveyReviewViewChecks
{
    public static void PulseSurveyReviewViewShowsTheStatusInCapitals()
    {
        var item = PulseSurveyReview.NewPulseSurveyReview("code-703", "label-588");
        var view = PulseSurveyReviewView.ToPulseSurveyReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PulseSurveyReviewView.ToPulseSurveyReviewViews(new List<PulseSurveyReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PulseSurveyReviewViewShowsTheStatusInCapitals();
    }
}
