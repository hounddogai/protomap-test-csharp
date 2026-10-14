namespace Acme.Workforce.Engagement.Tests.TownHall.Exception;

using Acme.Workforce.Engagement.TownHall.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class TownHallTemplateViewChecks
{
    public static void TownHallTemplateViewShowsTheStatusInCapitals()
    {
        var item = TownHallTemplate.NewTownHallTemplate("code-796", "label-155");
        var view = TownHallTemplateView.ToTownHallTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TownHallTemplateView.ToTownHallTemplateViews(new List<TownHallTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TownHallTemplateViewShowsTheStatusInCapitals();
    }
}
