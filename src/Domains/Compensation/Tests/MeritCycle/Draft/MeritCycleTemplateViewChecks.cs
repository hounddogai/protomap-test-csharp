namespace Acme.Workforce.Compensation.Tests.MeritCycle.Draft;

using Acme.Workforce.Compensation.MeritCycle.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class MeritCycleTemplateViewChecks
{
    public static void MeritCycleTemplateViewShowsTheStatusInCapitals()
    {
        var item = MeritCycleTemplate.NewMeritCycleTemplate("code-941", "title-975");
        var view = MeritCycleTemplateView.ToMeritCycleTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MeritCycleTemplateView.ToMeritCycleTemplateViews(new List<MeritCycleTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MeritCycleTemplateViewShowsTheStatusInCapitals();
    }
}
