namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Agreement;

using Acme.Workforce.Engagement.PulseSurvey.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class PulseSurveySummaryViewChecks
{
    public static void PulseSurveySummaryViewShowsTheStatusInCapitals()
    {
        var item = PulseSurveySummary.NewPulseSurveySummary("code-237", "name-286");
        var view = PulseSurveySummaryView.ToPulseSurveySummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PulseSurveySummaryView.ToPulseSurveySummaryViews(new List<PulseSurveySummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PulseSurveySummaryViewShowsTheStatusInCapitals();
    }
}
