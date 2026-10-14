namespace Acme.Workforce.Orgdesign.Tests.Position.Template;

using Acme.Workforce.Orgdesign.Position.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ScheduledPositionTemplateViewChecks
{
    public static void ScheduledPositionTemplateViewShowsTheStatusInCapitals()
    {
        var item = ScheduledPositionTemplate.NewScheduledPositionTemplate("code-222", "title-66");
        var view = ScheduledPositionTemplateView.ToScheduledPositionTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledPositionTemplateView.ToScheduledPositionTemplateViews(new List<ScheduledPositionTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledPositionTemplateViewShowsTheStatusInCapitals();
    }
}
