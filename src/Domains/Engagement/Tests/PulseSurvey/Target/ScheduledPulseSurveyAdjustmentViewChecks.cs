namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Target;

using Acme.Workforce.Engagement.PulseSurvey.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class ScheduledPulseSurveyAdjustmentViewChecks
{
    public static void ScheduledPulseSurveyAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = ScheduledPulseSurveyAdjustment.NewScheduledPulseSurveyAdjustment("code-955", "title-421");
        var view = ScheduledPulseSurveyAdjustmentView.ToScheduledPulseSurveyAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledPulseSurveyAdjustmentView.ToScheduledPulseSurveyAdjustmentViews(new List<ScheduledPulseSurveyAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledPulseSurveyAdjustmentViewShowsTheStatusInCapitals();
    }
}
