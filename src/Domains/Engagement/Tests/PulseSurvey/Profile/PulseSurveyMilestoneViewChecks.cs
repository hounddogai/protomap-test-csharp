namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Profile;

using Acme.Workforce.Engagement.PulseSurvey.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PulseSurveyMilestoneViewChecks
{
    public static void PulseSurveyMilestoneViewShowsTheStatusInCapitals()
    {
        var item = PulseSurveyMilestone.NewPulseSurveyMilestone("code-332", "label-303");
        var view = PulseSurveyMilestoneView.ToPulseSurveyMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PulseSurveyMilestoneView.ToPulseSurveyMilestoneViews(new List<PulseSurveyMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PulseSurveyMilestoneViewShowsTheStatusInCapitals();
    }
}
