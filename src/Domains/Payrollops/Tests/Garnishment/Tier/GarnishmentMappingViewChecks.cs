namespace Acme.Workforce.Payrollops.Tests.Garnishment.Tier;

using Acme.Workforce.Payrollops.Garnishment.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class GarnishmentMappingViewChecks
{
    public static void GarnishmentMappingViewShowsTheStatusInCapitals()
    {
        var item = GarnishmentMapping.NewGarnishmentMapping("code-408", "name-415");
        var view = GarnishmentMappingView.ToGarnishmentMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GarnishmentMappingView.ToGarnishmentMappingViews(new List<GarnishmentMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GarnishmentMappingViewShowsTheStatusInCapitals();
    }
}
