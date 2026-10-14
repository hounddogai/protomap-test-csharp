namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeBudget;

using Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

/// <summary>Checks of badge budget code, run by the project's runner.</summary>
public static class CappedAccessBadgeTemplateViewChecks
{
    public static void CappedAccessBadgeTemplateViewShowsTheStatusInCapitals()
    {
        var item = CappedAccessBadgeTemplate.NewCappedAccessBadgeTemplate("code-50", "title-626");
        var view = CappedAccessBadgeTemplateView.ToCappedAccessBadgeTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedAccessBadgeTemplateView.ToCappedAccessBadgeTemplateViews(new List<CappedAccessBadgeTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedAccessBadgeTemplateViewShowsTheStatusInCapitals();
    }
}
