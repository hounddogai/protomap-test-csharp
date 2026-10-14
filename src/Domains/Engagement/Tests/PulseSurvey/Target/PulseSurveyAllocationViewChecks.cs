namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Target;

using Acme.Workforce.Engagement.PulseSurvey.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class PulseSurveyAllocationViewChecks
{
    public static void PulseSurveyAllocationViewShowsTheStatusInCapitals()
    {
        var item = PulseSurveyAllocation.NewPulseSurveyAllocation("code-452", "label-506");
        var view = PulseSurveyAllocationView.ToPulseSurveyAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PulseSurveyAllocationView.ToPulseSurveyAllocationViews(new List<PulseSurveyAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PulseSurveyAllocationViewShowsTheStatusInCapitals();
    }
}
