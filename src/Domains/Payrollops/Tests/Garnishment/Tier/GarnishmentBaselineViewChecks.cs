namespace Acme.Workforce.Payrollops.Tests.Garnishment.Tier;

using Acme.Workforce.Payrollops.Garnishment.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class GarnishmentBaselineViewChecks
{
    public static void GarnishmentBaselineViewShowsTheStatusInCapitals()
    {
        var item = GarnishmentBaseline.NewGarnishmentBaseline("code-836", "label-332");
        var view = GarnishmentBaselineView.ToGarnishmentBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GarnishmentBaselineView.ToGarnishmentBaselineViews(new List<GarnishmentBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GarnishmentBaselineViewShowsTheStatusInCapitals();
    }
}
