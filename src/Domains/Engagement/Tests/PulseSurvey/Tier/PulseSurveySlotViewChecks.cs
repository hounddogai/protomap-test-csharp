namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Tier;

using Acme.Workforce.Engagement.PulseSurvey.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class PulseSurveySlotViewChecks
{
    public static void PulseSurveySlotViewShowsTheStatusInCapitals()
    {
        var item = PulseSurveySlot.NewPulseSurveySlot("code-628", "name-637");
        var view = PulseSurveySlotView.ToPulseSurveySlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PulseSurveySlotView.ToPulseSurveySlotViews(new List<PulseSurveySlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PulseSurveySlotViewShowsTheStatusInCapitals();
    }
}
