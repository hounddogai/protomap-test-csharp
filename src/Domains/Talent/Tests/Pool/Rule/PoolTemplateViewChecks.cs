namespace Acme.Workforce.Talent.Tests.Pool.Rule;

using Acme.Workforce.Talent.Pool.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class PoolTemplateViewChecks
{
    public static void PoolTemplateViewShowsTheStatusInCapitals()
    {
        var item = PoolTemplate.NewPoolTemplate("code-260", "label-80");
        var view = PoolTemplateView.ToPoolTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PoolTemplateView.ToPoolTemplateViews(new List<PoolTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PoolTemplateViewShowsTheStatusInCapitals();
    }
}
