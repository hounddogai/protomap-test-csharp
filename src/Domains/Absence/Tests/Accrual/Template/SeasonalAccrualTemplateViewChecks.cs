namespace Acme.Workforce.Absence.Tests.Accrual.Template;

using Acme.Workforce.Absence.Accrual.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class SeasonalAccrualTemplateViewChecks
{
    public static void SeasonalAccrualTemplateViewShowsTheStatusInCapitals()
    {
        var item = SeasonalAccrualTemplate.NewSeasonalAccrualTemplate("code-552", "name-482");
        var view = SeasonalAccrualTemplateView.ToSeasonalAccrualTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalAccrualTemplateView.ToSeasonalAccrualTemplateViews(new List<SeasonalAccrualTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalAccrualTemplateViewShowsTheStatusInCapitals();
    }
}
