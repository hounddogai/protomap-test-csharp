namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Assignment;

using Acme.Workforce.Engagement.PulseSurvey.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class PulseSurveyAssignmentViewChecks
{
    public static void PulseSurveyAssignmentViewShowsTheStatusInCapitals()
    {
        var item = PulseSurveyAssignment.NewPulseSurveyAssignment("code-591", "name-355");
        var view = PulseSurveyAssignmentView.ToPulseSurveyAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PulseSurveyAssignmentView.ToPulseSurveyAssignmentViews(new List<PulseSurveyAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PulseSurveyAssignmentViewShowsTheStatusInCapitals();
    }
}
