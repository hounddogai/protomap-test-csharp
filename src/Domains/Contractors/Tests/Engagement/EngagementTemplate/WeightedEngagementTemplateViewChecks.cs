namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementTemplate;

using Acme.Workforce.Contractors.Engagement.EngagementTemplate;

/// <summary>Checks of engagement template code, run by the project's runner.</summary>
public static class WeightedEngagementTemplateViewChecks
{
    public static void WeightedEngagementTemplateViewShowsTheStatusInCapitals()
    {
        var item = WeightedEngagementTemplate.NewWeightedEngagementTemplate("code-898", "title-370");
        var view = WeightedEngagementTemplateView.ToWeightedEngagementTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedEngagementTemplateView.ToWeightedEngagementTemplateViews(new List<WeightedEngagementTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedEngagementTemplateViewShowsTheStatusInCapitals();
    }
}
